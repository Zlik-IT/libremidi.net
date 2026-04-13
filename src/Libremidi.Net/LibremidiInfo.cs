namespace Libremidi.Net;

using Native;
using System.Runtime.InteropServices;

public static class LibremidiInfo
{
    static LibremidiInfo() => NativeLoader.EnsureLoaded();
    
    public static string Version => PtrToUtf8String(NativeMethods.GetVersion()) ?? "Unknown";

    public static bool TryGetApiDisplayName(string identifier, out string? displayName)
    {
        var api = NativeMethods.GetCompiledApiByIdentifier(identifier);
        if (api == LibremidiApi.Unspecified)
        {
            displayName = null;
            return false;
        }
        
        displayName = PtrToUtf8String(NativeMethods.GetApiDisplayName(api));
        return !string.IsNullOrWhiteSpace(displayName);
    }

    private static string? PtrToUtf8String(IntPtr ptr)
    {
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }
}