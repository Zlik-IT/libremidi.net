using Libremidi.Net.Native;

namespace Libremidi.Net;

/// <summary>Receives MIDI messages from an input port.</summary>
public sealed class MidiInput : IDisposable
{
    private bool _disposed;

    static MidiInput() => NativeLoader.EnsureLoaded();

    // Construction and P/Invoke wiring will be added when the C API bindings
    // are implemented in NativeMethods.cs.

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Release native handle here.
    }
}
