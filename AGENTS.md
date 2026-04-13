# libremidi.net Agent Guidelines

Welcome to the `libremidi.net` codebase! This document outlines key patterns, architecture, and developer workflows here to help AI coding agents be productive immediately.

## Architecture & Boundaries

`libremidi.net` is a thin .NET wrapper over the C++ [`libremidi`](https://github.com/celtera/libremidi) library using its C API. The project is split into two primary layers:
1. **`Libremidi.Net.Native`**: The low-level P/Invoke wrapper. Contains `NativeMethods.cs` where the `[DllImport]` or `[LibraryImport]` declarations for the C API reside. It also includes `NativeLoader.cs` which explicitly resolves the native shared library via `NativeLibrary.SetDllImportResolver` to handle self-contained apps and specialized deployment scenarios.
2. **`Libremidi.Net`**: The high-level, idiomatic .NET API containing classes like `MidiInput` and `MidiOutput`. This project consumes the `Native` project and translates C API concepts (like pointers and error codes) into modern C# concepts (like `IDisposable`, properties, and exceptions).

Target framework is **.NET 10** utilizing the latest C# language features (`LangVersion=latest`, `Nullable=enable`, `ImplicitUsings=enable`).

Current repository state is scaffold-first: `src/Libremidi.Net.Native/NativeMethods.cs` intentionally has no concrete P/Invoke entries yet, and the high-level API is currently minimal (`MidiInput`, `MidiOutput`, `MidiPort`).

## Conventions & Patterns

- **Native Interop**: Add C API P/Invoke signatures exclusively to `src/Libremidi.Net.Native/NativeMethods.cs`. Refer to `external/libremidi/include/libremidi/libremidi-c.h` when exposing new methods.
- **Scaffold-first Extension**: Add the smallest required C API surface in `NativeMethods.cs` first, then wire the matching high-level behavior in `src/Libremidi.Net`.
- **Resource Management**: High-level wrapper classes (e.g., `MidiInput`, `MidiOutput`) must implement `IDisposable` to properly free native handles allocated by the C API.
- **Native Loader Initialization**: High-level classes must trigger `NativeLoader.EnsureLoaded()` in their static constructors to ensure the DllImport resolver is installed before any P/Invoke calls are made.
- **NuGet Packaging Boundary**: `src/Libremidi.Net/Libremidi.Net.csproj` is the packable project and bundles `runtimes/**`; keep `src/Libremidi.Net.Native/Libremidi.Net.Native.csproj` non-packable.

## Build Process & Native Workflow

The native binaries are generated via a CMake build process orchestrated from `build/CMakeLists.txt`. 
- It configures `libremidi` from git submodule at `external/libremidi`.
- It sets `LIBREMIDI_C_API=ON` and `BUILD_SHARED_LIBS=ON` to produce a `.dll` / `.so` / `.dylib`.
- Output is routed into the standard .NET RID-specific directories (`runtimes/<rid>/native/`).
- It pins `RUNTIME_OUTPUT_DIRECTORY_*` and `LIBRARY_OUTPUT_DIRECTORY_*` to `OUTPUT_DIR` to avoid extra `Debug/Release` subfolders with multi-config generators.

When modifying native components or the CMake build, test the build specifically to verify that the native library compiles properly and drops into the expected `OUTPUT_DIR`.

Use the .NET SDK pinned in `global.json` (`10.0.104`) for local build/pack tasks to avoid toolchain drift.

## Relevant `dotnet-skills` Agents
When interacting with AI agents during development, utilize these specialist instructions (from the global guidance):
- `modern-csharp-coding-standards` & `api-design` when modifying `Libremidi.Net` classes.
- `type-design-performance` for optimizing P/Invoke structures in `Libremidi.Net.Native`.

