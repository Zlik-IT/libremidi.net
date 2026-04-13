namespace Libremidi.Net.Native;

using System;
using System.Runtime.InteropServices;

internal enum LibremidiConfigurationType
{
    Observer = 0,
    Input = 1,
    Output = 2,
}

internal enum LibremidiMidiVersion
{
    Midi1 = 1 << 1,
    Midi1Raw = 1 << 2,
    Midi2 = 1 << 3,
    Midi2Raw = 1 << 4,
}

internal enum LibremidiTimestampMode
{
    NoTimestamp = 0,
    Relative = 1,
    Absolute = 2,
    SystemMonotonic = 3,
    AudioFrame = 4,
    Custom = 5,
}

[StructLayout(LayoutKind.Sequential)]
internal struct LibremidiApiConfiguration
{
    internal LibremidiApi Api;
    internal LibremidiConfigurationType ConfigurationType;
    internal IntPtr Data;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LibremidiCallbackTarget
{
    internal IntPtr Context;
    internal IntPtr Callback;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LibremidiObserverConfiguration
{
    internal LibremidiCallbackTarget OnError;
    internal LibremidiCallbackTarget OnWarning;
    internal LibremidiCallbackTarget InputAdded;
    internal LibremidiCallbackTarget InputRemoved;
    internal LibremidiCallbackTarget OutputAdded;
    internal LibremidiCallbackTarget OutputRemoved;
    internal byte TrackHardware;
    internal byte TrackVirtual;
    internal byte TrackAny;
    internal byte NotifyInConstructor;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LibremidiMidiConfiguration
{
    internal LibremidiMidiVersion Version;
    internal IntPtr Port;
    internal LibremidiCallbackTarget MessageCallback;
    internal LibremidiCallbackTarget GetTimestamp;
    internal LibremidiCallbackTarget OnError;
    internal LibremidiCallbackTarget OnWarning;
    internal IntPtr PortName;
    internal byte VirtualPort;
    internal byte IgnoreSysex;
    internal byte IgnoreTiming;
    internal byte IgnoreSensing;
    internal LibremidiTimestampMode Timestamps;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void LibremidiInputPortCallback(IntPtr context, IntPtr inputPort);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void LibremidiMidi1MessageCallback(IntPtr context, long timestamp, IntPtr message, nuint length);

