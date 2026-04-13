namespace Libremidi.Net;

/// <summary>Represents a MIDI port available on the system.</summary>
public sealed class MidiPort
{
    internal MidiPort(int index, string name, ulong nativeHandle)
    {
        Index = index;
        Name = name;
        NativeHandle = nativeHandle;
    }

    /// <summary>Zero-based port index as reported by the backend.</summary>
    public int Index { get; }

    /// <summary>Human-readable port name.</summary>
    public string Name { get; }

    internal ulong NativeHandle { get; }

    public override string ToString() => $"{Index}: {Name}";
}
