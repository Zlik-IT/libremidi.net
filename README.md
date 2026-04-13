# libremidi.net

libremidi.net is as thin a wrapper as possible around [libremidi](https://github.com/celtera/libremidi).

## Build native library locally

Use the helper script to ensure CMake is present, configure the native build, and emit binaries to `runtimes/<rid>/native`.
If you omit `--rid`, the script auto-detects the host RID for Linux, macOS, and Windows Bash environments:

```bash
./build-native.sh
```

Optional examples:

```bash
./build-native.sh --rid linux-x64
./build-native.sh --rid osx-arm64
./build-native.sh --rid win-x64
./build-native.sh --rid linux-arm64 --build-type Debug
```

Run xUnit tests (with native library loading from `runtimes/<rid>/native`) via:

```bash
./test-native.sh
```

Optional test examples:

```bash
./test-native.sh --rid linux-x64
./test-native.sh --rid osx-arm64
./test-native.sh --rid win-x64
./test-native.sh --project tests/Libremidi.Net.SmokeTest/Libremidi.Net.SmokeTest.csproj
```

