namespace Libremidi.Net;

using Native;

public static class LibremidiInfo
{
    static LibremidiInfo() => NativeLoader.EnsureLoaded();
    
    public static string Version => NativeMethods.GetVersion() ?? "Unknown";
}