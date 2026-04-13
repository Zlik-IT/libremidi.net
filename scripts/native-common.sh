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

detect_host_os() {
  local os
  os="$(uname -s)"

  case "$os" in
    Linux) printf 'linux\n' ;;
    Darwin) printf 'osx\n' ;;
    MINGW*|MSYS*|CYGWIN*) printf 'win\n' ;;
    *) native_fail "Unsupported operating system '${os}'. Pass --rid explicitly if you are targeting a supported platform." ;;
  esac
}

detect_host_arch() {
  local arch
  arch="$(uname -m)"

  case "$arch" in
    x86_64|amd64) printf 'x64\n' ;;
    aarch64|arm64) printf 'arm64\n' ;;
    *) native_fail "Unsupported architecture '${arch}'. Pass --rid explicitly." ;;
  esac
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
  local host_os
  host_os="$(detect_host_os)"

  if [[ "$host_os" != "linux" ]]; then
    case "$host_os" in
      osx) native_fail "cmake is not installed. On macOS, install it first (for example: 'brew install cmake')." ;;
      win) native_fail "cmake is not installed. On Windows, install it first (for example: 'winget install Kitware.CMake' or use Visual Studio's CMake tools)." ;;
    esac
  fi

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
  local host_os
  host_os="$(detect_host_os)"

  if [[ "$host_os" != "linux" ]]; then
    case "$host_os" in
      osx) native_fail "A C++ compiler is not installed. On macOS, install Xcode Command Line Tools first (for example: 'xcode-select --install')." ;;
      win) native_fail "A C++ compiler is not installed. On Windows, install Visual Studio Build Tools or a compatible MSVC/Clang toolchain first." ;;
    esac
  fi

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

detect_default_rid() {
  local host_os host_arch
  host_os="$(detect_host_os)"
  host_arch="$(detect_host_arch)"

  printf '%s-%s\n' "$host_os" "$host_arch"
}

resolve_native_paths() {
  local rid="$1"
  printf '%s;%s\n' "${REPO_ROOT}/build/out/${rid}" "${REPO_ROOT}/runtimes/${rid}/native"
}

get_native_library_path_var() {
  case "$(detect_host_os)" in
    linux) printf 'LD_LIBRARY_PATH\n' ;;
    osx) printf 'DYLD_LIBRARY_PATH\n' ;;
    win) printf 'PATH\n' ;;
  esac
}

get_path_separator() {
  case "$(detect_host_os)" in
    win) printf ';\n' ;;
    *) printf ':\n' ;;
  esac
}

prepend_native_library_path() {
  local native_dir="$1"
  local env_var separator current_value updated_value

  env_var="$(get_native_library_path_var)"
  separator="$(get_path_separator)"
  current_value="${!env_var:-}"

  if [[ -n "$current_value" ]]; then
    updated_value="${native_dir}${separator}${current_value}"
  else
    updated_value="$native_dir"
  fi

  printf '%s=%s\n' "$env_var" "$updated_value"
}

