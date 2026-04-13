namespace Libremidi.Net.Native;

using System;
using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
    [DllImport(LibName, EntryPoint = "libremidi_midi_in_new")]
    internal static extern int CreateMidiIn(
        ref LibremidiMidiConfiguration midiConfiguration,
        ref LibremidiApiConfiguration apiConfiguration,
        out IntPtr midiInHandle);

    [DllImport(LibName, EntryPoint = "libremidi_midi_in_is_connected")]
    internal static extern int IsMidiInConnected(IntPtr midiInHandle);

    [DllImport(LibName, EntryPoint = "libremidi_midi_in_free")]
    internal static extern int FreeMidiIn(IntPtr midiInHandle);
}
