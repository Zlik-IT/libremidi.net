namespace Libremidi.Net.Native;

using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
    private const string LibName = "libremidi";

    // P/Invoke declarations will be added here as the libremidi C API is wrapped.
    // Reference: external/libremidi/include/libremidi/libremidi-c.h

    [LibraryImport(LibName, EntryPoint = "libremidi_get_version")]
    internal static partial IntPtr GetVersion();

    // const char* -> enum libremidi_api
    [LibraryImport(
        LibName,
        EntryPoint = "libremidi_get_compiled_api_by_identifier",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial LibremidiApi GetCompiledApiByIdentifier(string identifier);

    // enum libremidi_api -> const char*
    [LibraryImport(
        LibName,
        EntryPoint = "libremidi_api_display_name")]
    internal static partial IntPtr GetApiDisplayName(LibremidiApi api);
}