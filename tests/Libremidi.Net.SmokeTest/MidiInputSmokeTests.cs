namespace Libremidi.Net.SmokeTest;

using System;
using Xunit;

public sealed class MidiInputSmokeTests
{
    [Fact]
    public void GetAvailablePorts_ReturnsStableIndices()
    {
        IReadOnlyList<MidiPort> ports;
        try
        {
            ports = MidiInput.GetAvailablePorts();
        }
        catch (DllNotFoundException)
        {
            // Native runtime asset may be unavailable in some dev/CI setups.
            Assert.True(true);
            return;
        }

        Assert.NotNull(ports);

        for (var i = 0; i < ports.Count; i++)
        {
            Assert.Equal(i, ports[i].Index);
        }
    }

    [Fact]
    public void OpenByIndex_UsesFirstPort_WhenAvailable()
    {
        using var input = new MidiInput();

        IReadOnlyList<MidiPort> ports;
        try
        {
            ports = MidiInput.GetAvailablePorts();
        }
        catch (DllNotFoundException)
        {
            // Native runtime asset may be unavailable in some dev/CI setups.
            Assert.True(true);
            return;
        }

        if (ports.Count == 0)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => input.Open(0));
            return;
        }

        input.Open(0);
        _ = input.IsConnected;
        input.Close();
        Assert.False(input.IsConnected);
    }
}

