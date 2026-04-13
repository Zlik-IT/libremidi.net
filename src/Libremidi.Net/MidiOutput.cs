using Libremidi.Net.Native;

namespace Libremidi.Net;

/// <summary>Sends MIDI messages to an output port.</summary>
public sealed class MidiOutput : IDisposable
{
    private bool _disposed;

    static MidiOutput() => NativeLoader.EnsureLoaded();

    // Construction and P/Invoke wiring will be added when the C API bindings
    // are implemented in NativeMethods.cs.

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Release native handle here.
    }
}
