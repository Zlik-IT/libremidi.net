# libremidi.net Agent Guidelines

Welcome to the `libremidi.net` codebase! This document outlines key patterns, architecture, and developer workflows here to help AI coding agents be productive immediately.

## Architecture & Boundaries

`libremidi.net` is a thin .NET wrapper over the C++ [`libremidi`](https://github.com/celtera/libremidi) library using its C API. The project is split into two primary layers:
1. **`Libremidi.Net.Native`**: The low-level P/Invoke wrapper. Contains `NativeMethods.cs` where the `[DllImport]` or `[LibraryImport]` declarations for the C API reside. It also includes `NativeLoader.cs` which explicitly resolves the native shared library via `NativeLibrary.SetDllImportResolver` to handle self-contained apps and specialized deployment scenarios.
2. **`Libremidi.Net`**: The high-level, idiomatic .NET API containing classes like `MidiInput` and `MidiOutput`. This project consumes the `Native` project and translates C API concepts (like pointers and error codes) into modern C# concepts (like `IDisposable`, properties, and exceptions).

Target framework is **.NET 10** utilizing the latest C# language features (`LangVersion=latest`, `Nullable=enable`, `ImplicitUsings=enable`).

## Conventions & Patterns

- **Native Interop**: Add C API P/Invoke signatures exclusively to `src/Libremidi.Net.Native/NativeMethods.cs`. Refer to `external/libremidi/include/libremidi/libremidi-c.h` when exposing new methods.
- **Resource Management**: High-level wrapper classes (e.g., `MidiInput`, `MidiOutput`) must implement `IDisposable` to properly free native handles allocated by the C API.
- **Native Loader Initialization**: High-level classes must trigger `NativeLoader.EnsureLoaded()` in their static constructors to ensure the DllImport resolver is installed before any P/Invoke calls are made.

## Build Process & Native Workflow

The native binaries are generated via a CMake build process orchestrated from `build/CMakeLists.txt`. 
- It configures `libremidi` from git submodule at `external/libremidi`.
- It sets `LIBREMIDI_C_API=ON` and `BUILD_SHARED_LIBS=ON` to produce a `.dll` / `.so` / `.dylib`.
- Output is routed into the standard .NET RID-specific directories (`runtimes/<rid>/native/`).

When modifying native components or the CMake build, test the build specifically to verify that the native library compiles properly and drops into the expected `OUTPUT_DIR`.

## Relevant `dotnet-skills` Agents
When interacting with AI agents during development, utilize these specialist instructions (from the global guidance):
- `modern-csharp-coding-standards` & `api-design` when modifying `Libremidi.Net` classes.
- `type-design-performance` for optimizing P/Invoke structures in `Libremidi.Net.Native`.

