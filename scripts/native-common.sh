#!/usr/bin/env bash
set -euo pipefail

REQUIRED_CMAKE_VERSION="3.21.0"
DEFAULT_BUILD_TYPE="Release"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

native_log() {
  printf '[native] %s\n' "$*"
}

native_fail() {
  printf '[native] ERROR: %s\n' "$*" >&2
  exit 1
}

version_at_least() {
  local actual="$1"
  local required="$2"
  [[ "$(printf '%s\n' "$required" "$actual" | sort -V | head -n1)" == "$required" ]]
}

run_installer() {
  local installer="$1"

  if [[ "${EUID}" -eq 0 ]]; then
    bash -lc "$installer"
  elif command -v sudo >/dev/null 2>&1; then
    sudo bash -lc "$installer"
  else
    native_fail "Installation requires root or sudo."
  fi
}

install_cmake() {
  local installer=""

  if command -v apt-get >/dev/null 2>&1; then
    installer="apt-get update && apt-get install -y cmake"
  elif command -v dnf >/dev/null 2>&1; then
    installer="dnf install -y cmake"
  elif command -v yum >/dev/null 2>&1; then
    installer="yum install -y cmake"
  elif command -v pacman >/dev/null 2>&1; then
    installer="pacman -Sy --noconfirm cmake"
  elif command -v zypper >/dev/null 2>&1; then
    installer="zypper --non-interactive install cmake"
  elif command -v apk >/dev/null 2>&1; then
    installer="apk add --no-cache cmake"
  else
    native_fail "Could not find a supported package manager to install cmake."
  fi

  native_log "Installing cmake"
  run_installer "$installer"
}

ensure_cmake() {
  if ! command -v cmake >/dev/null 2>&1; then
    native_log "cmake not found; attempting installation"
    install_cmake
  fi

  local actual
  actual="$(cmake --version | awk 'NR==1 {print $3}')"
  if ! version_at_least "$actual" "$REQUIRED_CMAKE_VERSION"; then
    native_fail "cmake version ${actual} is too old; need >= ${REQUIRED_CMAKE_VERSION}."
  fi

  native_log "Using cmake ${actual}"
}

install_build_tools() {
  local installer=""

  if command -v apt-get >/dev/null 2>&1; then
    installer="apt-get update && apt-get install -y build-essential"
  elif command -v dnf >/dev/null 2>&1; then
    installer="dnf install -y gcc-c++ make"
  elif command -v yum >/dev/null 2>&1; then
    installer="yum install -y gcc-c++ make"
  elif command -v pacman >/dev/null 2>&1; then
    installer="pacman -Sy --noconfirm base-devel"
  elif command -v zypper >/dev/null 2>&1; then
    installer="zypper --non-interactive install gcc-c++ make"
  elif command -v apk >/dev/null 2>&1; then
    installer="apk add --no-cache g++ make"
  else
    native_fail "Could not find a supported package manager to install C++ build tools."
  fi

  native_log "Installing C++ build tools"
  run_installer "$installer"
}

ensure_cpp_compiler() {
  if command -v c++ >/dev/null 2>&1 || command -v g++ >/dev/null 2>&1 || command -v clang++ >/dev/null 2>&1; then
    return
  fi

  native_log "No C++ compiler found; attempting installation"
  install_build_tools

  if ! command -v c++ >/dev/null 2>&1 && ! command -v g++ >/dev/null 2>&1 && ! command -v clang++ >/dev/null 2>&1; then
    native_fail "A C++ compiler is still unavailable after installation."
  fi
}

ensure_submodule() {
  if [[ -f "${REPO_ROOT}/external/libremidi/CMakeLists.txt" ]]; then
    return
  fi

  if ! command -v git >/dev/null 2>&1; then
    native_fail "external/libremidi is missing and git is not available to fetch submodules."
  fi

  native_log "Initializing external/libremidi submodule"
  git -C "${REPO_ROOT}" submodule update --init --recursive external/libremidi
}

detect_linux_rid() {
  local os arch
  os="$(uname -s)"
  arch="$(uname -m)"

  if [[ "$os" != "Linux" ]]; then
    native_fail "This helper currently supports Linux only. Pass --rid manually for other platforms."
  fi

  case "$arch" in
    x86_64) printf 'linux-x64\n' ;;
    aarch64|arm64) printf 'linux-arm64\n' ;;
    *) native_fail "Unsupported architecture '${arch}'. Pass --rid manually." ;;
  esac
}

resolve_native_paths() {
  local rid="$1"
  printf '%s;%s\n' "${REPO_ROOT}/build/out/${rid}" "${REPO_ROOT}/runtimes/${rid}/native"
}

