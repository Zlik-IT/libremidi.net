namespace Libremidi.Net;

using Native;

public static class LibremidiInfo
{
    static LibremidiInfo() => NativeLoader.EnsureLoaded();
    
    public static string Version => NativeMethods.GetVersion() ?? "Unknown";

    public static bool TryGetApiDisplayName(string identifier, out string? displayName)
    {
        var api = NativeMethods.GetCompiledApiByIdentifier(identifier);
        if (api == LibremidiApi.Unspecified)
        {
            displayName = null;
            return false;
        }
        
        displayName = NativeMethods.GetApiDisplayName(api);
        return true;
    }
}