#!/usr/bin/env bash
set -euo pipefail

REQUIRED_CMAKE_VERSION="3.21.0"
BUILD_TYPE="Release"
RID=""
RUN_SMOKE_TEST="false"
SMOKE_TEST_PROJECT=""

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="${SCRIPT_DIR}"

usage() {
  cat <<'EOF'
Usage: ./build-native.sh [options]

Options:
  --rid <rid>           Runtime identifier to build (default: auto-detect)
  --build-type <type>   CMake build type (default: Release)
  --run-smoke-test      Run smoke test project after native build
  --smoke-project <csproj>
                        Override smoke test project path
  -h, --help            Show this help

Examples:
  ./build-native.sh
  ./build-native.sh --rid linux-x64
  ./build-native.sh --rid linux-arm64 --build-type Debug
  ./build-native.sh --run-smoke-test
EOF
}

log() {
  printf '[build-native] %s\n' "$*"
}

fail() {
  printf '[build-native] ERROR: %s\n' "$*" >&2
  exit 1
}

version_at_least() {
  local actual="$1"
  local required="$2"
  [[ "$(printf '%s\n' "$required" "$actual" | sort -V | head -n1)" == "$required" ]]
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
    fail "Could not find a supported package manager to install cmake. Install cmake >= ${REQUIRED_CMAKE_VERSION} manually."
  fi

  if [[ "${EUID}" -eq 0 ]]; then
    log "Installing cmake as root"
    bash -lc "$installer"
  elif command -v sudo >/dev/null 2>&1; then
    log "Installing cmake with sudo"
    sudo bash -lc "$installer"
  else
    fail "cmake is missing and sudo is not available. Install cmake >= ${REQUIRED_CMAKE_VERSION} manually."
  fi
}

ensure_cmake() {
  if ! command -v cmake >/dev/null 2>&1; then
    log "cmake not found; attempting installation"
    install_cmake
  fi

  local actual
  actual="$(cmake --version | awk 'NR==1 {print $3}')"
  if ! version_at_least "$actual" "$REQUIRED_CMAKE_VERSION"; then
    fail "cmake version ${actual} is too old; need >= ${REQUIRED_CMAKE_VERSION}."
  fi

  log "Using cmake ${actual}"
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
    fail "Could not find a supported package manager to install a C++ compiler. Install one manually and re-run."
  fi

  if [[ "${EUID}" -eq 0 ]]; then
    log "Installing C++ build tools as root"
    bash -lc "$installer"
  elif command -v sudo >/dev/null 2>&1; then
    log "Installing C++ build tools with sudo"
    sudo bash -lc "$installer"
  else
    fail "C++ compiler is missing and sudo is not available. Install build tools manually and re-run."
  fi
}

ensure_cpp_compiler() {
  if command -v c++ >/dev/null 2>&1 || command -v g++ >/dev/null 2>&1 || command -v clang++ >/dev/null 2>&1; then
    return
  fi

  log "No C++ compiler found; attempting installation"
  install_build_tools

  if ! command -v c++ >/dev/null 2>&1 && ! command -v g++ >/dev/null 2>&1 && ! command -v clang++ >/dev/null 2>&1; then
    fail "A C++ compiler is still unavailable after installation."
  fi
}

detect_rid() {
  local os arch
  os="$(uname -s)"
  arch="$(uname -m)"

  if [[ "$os" != "Linux" ]]; then
    fail "This helper currently supports Linux only. Pass --rid manually from CI for other platforms."
  fi

  case "$arch" in
    x86_64) RID="linux-x64" ;;
    aarch64|arm64) RID="linux-arm64" ;;
    *) fail "Unsupported architecture '${arch}'. Pass --rid manually." ;;
  esac
}

ensure_submodule() {
  if [[ -f "${REPO_ROOT}/external/libremidi/CMakeLists.txt" ]]; then
    return
  fi

  if ! command -v git >/dev/null 2>&1; then
    fail "external/libremidi is missing and git is not available to fetch submodules."
  fi

  log "Initializing external/libremidi submodule"
  git -C "${REPO_ROOT}" submodule update --init --recursive external/libremidi
}

run_smoke_test() {
  if ! command -v dotnet >/dev/null 2>&1; then
    fail "dotnet SDK is required to run smoke tests."
  fi

  local project_path="${SMOKE_TEST_PROJECT:-${REPO_ROOT}/tests/Libremidi.Net.SmokeTest/Libremidi.Net.SmokeTest.csproj}"
  if [[ ! -f "$project_path" ]]; then
    fail "Smoke test project not found: ${project_path}"
  fi

  log "Running smoke test project: ${project_path}"
  LD_LIBRARY_PATH="${OUTPUT_DIR}:${LD_LIBRARY_PATH:-}" \
    dotnet test "$project_path" -c "$BUILD_TYPE" --logger "console;verbosity=normal"
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --rid)
      [[ $# -lt 2 ]] && fail "--rid expects a value"
      RID="$2"
      shift 2
      ;;
    --build-type)
      [[ $# -lt 2 ]] && fail "--build-type expects a value"
      BUILD_TYPE="$2"
      shift 2
      ;;
    --run-smoke-test)
      RUN_SMOKE_TEST="true"
      shift
      ;;
    --smoke-project)
      [[ $# -lt 2 ]] && fail "--smoke-project expects a value"
      SMOKE_TEST_PROJECT="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      fail "Unknown argument: $1"
      ;;
  esac
done

ensure_cmake
ensure_cpp_compiler
ensure_submodule

if [[ -z "$RID" ]]; then
  detect_rid
fi

BUILD_DIR="${REPO_ROOT}/build/out/${RID}"
OUTPUT_DIR="${REPO_ROOT}/runtimes/${RID}/native"

mkdir -p "$OUTPUT_DIR"

log "Configuring CMake for RID ${RID}"
cmake -S "${REPO_ROOT}/build" -B "$BUILD_DIR" \
  -DCMAKE_BUILD_TYPE="$BUILD_TYPE" \
  -DOUTPUT_DIR="$OUTPUT_DIR"

log "Building native library"
cmake --build "$BUILD_DIR" --config "$BUILD_TYPE" --parallel

log "Build complete. Output files in ${OUTPUT_DIR}:"
find "$OUTPUT_DIR" -maxdepth 1 -type f | sed 's#^#  - #' || true

if [[ "$RUN_SMOKE_TEST" == "true" ]]; then
  run_smoke_test
fi

