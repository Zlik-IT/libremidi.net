#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/scripts/native-common.sh"

BUILD_TYPE="${DEFAULT_BUILD_TYPE}"
RID=""
TEST_PROJECT="${REPO_ROOT}/tests/Libremidi.Net.SmokeTest/Libremidi.Net.SmokeTest.csproj"

usage() {
  cat <<'EOF'
Usage: ./test-native.sh [options]

Options:
  --rid <rid>           Runtime identifier to test (default: auto-detect)
  --build-type <type>   dotnet test configuration (default: Release)
  --project <csproj>    Test project path (default: tests/Libremidi.Net.SmokeTest/...)
  -h, --help            Show this help

Examples:
  ./test-native.sh
  ./test-native.sh --rid linux-x64
  ./test-native.sh --project tests/Libremidi.Net.SmokeTest/Libremidi.Net.SmokeTest.csproj
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --rid)
      [[ $# -lt 2 ]] && native_fail "--rid expects a value"
      RID="$2"
      shift 2
      ;;
    --build-type)
      [[ $# -lt 2 ]] && native_fail "--build-type expects a value"
      BUILD_TYPE="$2"
      shift 2
      ;;
    --project)
      [[ $# -lt 2 ]] && native_fail "--project expects a value"
      TEST_PROJECT="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      native_fail "Unknown argument: $1"
      ;;
  esac
done

if ! command -v dotnet >/dev/null 2>&1; then
  native_fail "dotnet SDK is required to run tests."
fi

if [[ -z "$RID" ]]; then
  RID="$(detect_linux_rid)"
fi

if [[ ! -f "$TEST_PROJECT" ]]; then
  native_fail "Test project not found: ${TEST_PROJECT}"
fi

IFS=';' read -r _ OUTPUT_DIR <<<"$(resolve_native_paths "$RID")"
if [[ ! -d "$OUTPUT_DIR" ]]; then
  native_fail "Native output directory does not exist: ${OUTPUT_DIR}. Run ./build-native.sh first."
fi

native_log "test-native: running ${TEST_PROJECT} for ${RID}"
LD_LIBRARY_PATH="${OUTPUT_DIR}:${LD_LIBRARY_PATH:-}" \
  dotnet test "$TEST_PROJECT" -c "$BUILD_TYPE" --logger "console;verbosity=normal"

