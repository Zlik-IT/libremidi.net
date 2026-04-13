namespace Libremidi.Net.Native;

using System;
using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
    [LibraryImport(LibName, EntryPoint = "libremidi_midi_in_port_clone")]
    internal static partial int CloneMidiInPort(IntPtr inputPort, out IntPtr destinationPort);

    [LibraryImport(LibName, EntryPoint = "libremidi_midi_in_port_free")]
    internal static partial int FreeMidiInPort(IntPtr inputPort);

    [LibraryImport(LibName, EntryPoint = "libremidi_midi_in_port_name")]
    internal static partial int GetMidiInPortName(IntPtr inputPort, out IntPtr name, out nuint length);

    [LibraryImport(LibName, EntryPoint = "libremidi_midi_in_port_handle")]
    internal static partial int GetMidiInPortHandle(IntPtr inputPort, out ulong handle);
}

