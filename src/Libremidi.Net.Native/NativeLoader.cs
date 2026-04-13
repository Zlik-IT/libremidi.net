using System.Reflection;
using System.Runtime.InteropServices;

namespace Libremidi.Net.Native;

/// <summary>
/// Ensures the native libremidi shared library is resolvable at runtime.
/// The .NET runtime resolves RID-based libraries automatically when bundled
/// in a NuGet package; this class handles edge cases (e.g. self-contained apps).
/// </summary>
internal static class NativeLoader
{
    static NativeLoader()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeLoader).Assembly, Resolve);
    }

    /// <summary>Call once at startup to trigger the static constructor.</summary>
    internal static void EnsureLoaded() { }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // Let the default resolver handle it; override here if needed.
        return IntPtr.Zero;
    }
}
