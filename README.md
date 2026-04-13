# libremidi.net

libremidi.net is as thin a wrapper as possible around [libremidi](https://github.com/celtera/libremidi).

## Build native library locally

Use the helper script to ensure CMake is present, configure the native build, and emit binaries to `runtimes/<rid>/native`:

```bash
./build-native.sh
```

Optional examples:

```bash
./build-native.sh --rid linux-x64
./build-native.sh --rid linux-arm64 --build-type Debug
```
