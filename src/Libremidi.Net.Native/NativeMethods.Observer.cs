namespace Libremidi.Net.Native;

using System;
using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
    [DllImport(LibName, EntryPoint = "libremidi_midi_observer_new")]
    internal static extern int CreateMidiObserver(
        ref LibremidiObserverConfiguration observerConfiguration,
        ref LibremidiApiConfiguration apiConfiguration,
        out IntPtr observerHandle);

    [DllImport(LibName, EntryPoint = "libremidi_midi_observer_enumerate_input_ports")]
    internal static extern int EnumerateInputPorts(
        IntPtr observerHandle,
        IntPtr context,
        IntPtr callback);

    [DllImport(LibName, EntryPoint = "libremidi_midi_observer_free")]
    internal static extern int FreeMidiObserver(IntPtr observerHandle);
}
