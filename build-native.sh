#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/scripts/native-common.sh"

BUILD_TYPE="${DEFAULT_BUILD_TYPE}"
RID=""

usage() {
  cat <<'EOF'
Usage: ./build-native.sh [options]

Options:
  --rid <rid>           Runtime identifier to build (default: auto-detect)
  --build-type <type>   CMake build type (default: Release)
  -h, --help            Show this help

Examples:
  ./build-native.sh
  ./build-native.sh --rid linux-x64
  ./build-native.sh --rid linux-arm64 --build-type Debug
EOF
}

log() {
  native_log "build-native: $*"
}

fail() {
  native_fail "build-native: $*"
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
  RID="$(detect_linux_rid)"
fi

IFS=';' read -r BUILD_DIR OUTPUT_DIR <<<"$(resolve_native_paths "$RID")"

mkdir -p "$OUTPUT_DIR"

log "Configuring CMake for RID ${RID}"
cmake -S "${REPO_ROOT}/build" -B "$BUILD_DIR" \
  -DCMAKE_BUILD_TYPE="$BUILD_TYPE" \
  -DOUTPUT_DIR="$OUTPUT_DIR"

log "Building native library"
cmake --build "$BUILD_DIR" --config "$BUILD_TYPE" --parallel

log "Build complete. Output files in ${OUTPUT_DIR}:"
find "$OUTPUT_DIR" -maxdepth 1 -type f | sed 's#^#  - #' || true
