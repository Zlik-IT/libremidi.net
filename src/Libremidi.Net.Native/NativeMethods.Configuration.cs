namespace Libremidi.Net.Native;

using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
    [DllImport(LibName, EntryPoint = "libremidi_midi_api_configuration_init")]
    internal static extern int InitializeMidiApiConfiguration(ref LibremidiApiConfiguration configuration);

    [DllImport(LibName, EntryPoint = "libremidi_midi_observer_configuration_init")]
    internal static extern int InitializeMidiObserverConfiguration(ref LibremidiObserverConfiguration configuration);

    [DllImport(LibName, EntryPoint = "libremidi_midi_configuration_init")]
    internal static extern int InitializeMidiConfiguration(ref LibremidiMidiConfiguration configuration);
}
