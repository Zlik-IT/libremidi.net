using Libremidi.Net.Native;
using System.Runtime.InteropServices;

namespace Libremidi.Net;

/// <summary>Receives MIDI messages from an input port.</summary>
public sealed class MidiInput : IDisposable
{
    private static readonly LibremidiInputPortCallback EnumerateInputPortCallback = OnEnumerateInputPort;
    private static readonly IntPtr EnumerateInputPortCallbackPtr = Marshal.GetFunctionPointerForDelegate(EnumerateInputPortCallback);
    private static readonly LibremidiMidi1MessageCallback NoOpMidi1MessageCallback = (_, _, _, _) => { };
    private static readonly IntPtr NoOpMidi1MessageCallbackPtr = Marshal.GetFunctionPointerForDelegate(NoOpMidi1MessageCallback);

    private IntPtr _midiInHandle;
    private bool _disposed;

    static MidiInput() => NativeLoader.EnsureLoaded();

    public bool IsConnected
    {
        get
        {
            ThrowIfDisposed();
            if (_midiInHandle == IntPtr.Zero)
            {
                return false;
            }

            var connected = NativeMethods.IsMidiInConnected(_midiInHandle);
            if (connected < 0)
            {
                NativeResult.ThrowIfFailed(connected, "libremidi_midi_in_is_connected");
            }

            return connected == 1;
        }
    }

    public static IReadOnlyList<MidiPort> GetAvailablePorts()
    {
        var ports = new List<MidiPort>();
        var context = new CollectPortsContext(ports);
        EnumerateInputPorts(context);

        if (context.Exception is not null)
        {
            throw context.Exception;
        }

        return ports;
    }

    public void Open(MidiPort port)
    {
        ArgumentNullException.ThrowIfNull(port);
        ThrowIfDisposed();

        CloseInternal(throwOnError: true);

        var selectedPort = FindPortByHandle(port.NativeHandle);
        if (selectedPort == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Input port '{port.Name}' is no longer available.");
        }

        try
        {
            var midiConfiguration = default(LibremidiMidiConfiguration);
            NativeResult.ThrowIfFailed(
                NativeMethods.InitializeMidiConfiguration(ref midiConfiguration),
                "libremidi_midi_configuration_init");

            midiConfiguration.Version = LibremidiMidiVersion.Midi1;
            midiConfiguration.Port = selectedPort;
            midiConfiguration.MessageCallback = new LibremidiCallbackTarget
            {
                Context = IntPtr.Zero,
                Callback = NoOpMidi1MessageCallbackPtr,
            };

            var apiConfiguration = default(LibremidiApiConfiguration);
            NativeResult.ThrowIfFailed(
                NativeMethods.InitializeMidiApiConfiguration(ref apiConfiguration),
                "libremidi_midi_api_configuration_init");

            apiConfiguration.ConfigurationType = LibremidiConfigurationType.Input;
            apiConfiguration.Api = LibremidiApi.Unspecified;

            NativeResult.ThrowIfFailed(
                NativeMethods.CreateMidiIn(ref midiConfiguration, ref apiConfiguration, out _midiInHandle),
                "libremidi_midi_in_new");
        }
        finally
        {
            NativeMethods.FreeMidiInPort(selectedPort);
        }
    }

    public void Open(int portIndex)
    {
        ThrowIfDisposed();

        var ports = GetAvailablePorts();
        if ((uint)portIndex >= (uint)ports.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(portIndex), portIndex, "No input port exists at the provided index.");
        }

        Open(ports[portIndex]);
    }

    public void Close()
    {
        ThrowIfDisposed();
        CloseInternal(throwOnError: true);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CloseInternal(throwOnError: false);
    }

    private static void EnumerateInputPorts(object managedContext)
    {
        var observerConfiguration = default(LibremidiObserverConfiguration);
        NativeResult.ThrowIfFailed(
            NativeMethods.InitializeMidiObserverConfiguration(ref observerConfiguration),
            "libremidi_midi_observer_configuration_init");

        observerConfiguration.TrackHardware = 1;
        observerConfiguration.TrackVirtual = 1;
        observerConfiguration.TrackAny = 1;

        var apiConfiguration = default(LibremidiApiConfiguration);
        NativeResult.ThrowIfFailed(
            NativeMethods.InitializeMidiApiConfiguration(ref apiConfiguration),
            "libremidi_midi_api_configuration_init");

        apiConfiguration.ConfigurationType = LibremidiConfigurationType.Observer;
        apiConfiguration.Api = LibremidiApi.Unspecified;

        NativeResult.ThrowIfFailed(
            NativeMethods.CreateMidiObserver(ref observerConfiguration, ref apiConfiguration, out var observerHandle),
            "libremidi_midi_observer_new");

        var handle = GCHandle.Alloc(managedContext);
        try
        {
            NativeResult.ThrowIfFailed(
                NativeMethods.EnumerateInputPorts(observerHandle, GCHandle.ToIntPtr(handle), EnumerateInputPortCallbackPtr),
                "libremidi_midi_observer_enumerate_input_ports");
        }
        finally
        {
            handle.Free();
            NativeMethods.FreeMidiObserver(observerHandle);
        }
    }

    private static IntPtr FindPortByHandle(ulong targetHandle)
    {
        var context = new FindPortContext(targetHandle);
        EnumerateInputPorts(context);

        if (context.Exception is not null)
        {
            throw context.Exception;
        }

        return context.SelectedPort;
    }

    private static void OnEnumerateInputPort(IntPtr context, IntPtr inputPort)
    {
        var gcHandle = GCHandle.FromIntPtr(context);
        if (gcHandle.Target is CollectPortsContext collectContext)
        {
            TryCollectPort(collectContext, inputPort);
            return;
        }

        if (gcHandle.Target is FindPortContext findContext)
        {
            TrySelectPort(findContext, inputPort);
        }
    }

    private static void TryCollectPort(CollectPortsContext context, IntPtr inputPort)
    {
        if (context.Exception is not null)
        {
            return;
        }

        IntPtr clonedPort = IntPtr.Zero;
        try
        {
            NativeResult.ThrowIfFailed(NativeMethods.CloneMidiInPort(inputPort, out clonedPort), "libremidi_midi_in_port_clone");
            NativeResult.ThrowIfFailed(NativeMethods.GetMidiInPortHandle(clonedPort, out var handle), "libremidi_midi_in_port_handle");
            NativeResult.ThrowIfFailed(NativeMethods.GetMidiInPortName(clonedPort, out var namePtr, out var nameLength), "libremidi_midi_in_port_name");

            var name = namePtr == IntPtr.Zero
                ? string.Empty
                : Marshal.PtrToStringUTF8(namePtr, checked((int)nameLength));
            context.Ports.Add(new MidiPort(context.Ports.Count, name, handle));
        }
        catch (Exception ex)
        {
            context.Exception = ex;
        }
        finally
        {
            if (clonedPort != IntPtr.Zero)
            {
                NativeMethods.FreeMidiInPort(clonedPort);
            }
        }
    }

    private static void TrySelectPort(FindPortContext context, IntPtr inputPort)
    {
        if (context.Exception is not null || context.SelectedPort != IntPtr.Zero)
        {
            return;
        }

        try
        {
            NativeResult.ThrowIfFailed(NativeMethods.GetMidiInPortHandle(inputPort, out var handle), "libremidi_midi_in_port_handle");
            if (handle != context.TargetHandle)
            {
                return;
            }

            NativeResult.ThrowIfFailed(NativeMethods.CloneMidiInPort(inputPort, out var clonedPort), "libremidi_midi_in_port_clone");
            context.SelectedPort = clonedPort;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
        }
    }

    private void CloseInternal(bool throwOnError)
    {
        if (_midiInHandle == IntPtr.Zero)
        {
            return;
        }

        var handle = _midiInHandle;
        _midiInHandle = IntPtr.Zero;

        var result = NativeMethods.FreeMidiIn(handle);
        if (throwOnError)
        {
            NativeResult.ThrowIfFailed(result, "libremidi_midi_in_free");
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MidiInput));
        }
    }

    private sealed class CollectPortsContext(List<MidiPort> ports)
    {
        internal List<MidiPort> Ports { get; } = ports;
        internal Exception? Exception { get; set; }
    }

    private sealed class FindPortContext(ulong targetHandle)
    {
        internal ulong TargetHandle { get; } = targetHandle;
        internal IntPtr SelectedPort { get; set; }
        internal Exception? Exception { get; set; }
    }
}
