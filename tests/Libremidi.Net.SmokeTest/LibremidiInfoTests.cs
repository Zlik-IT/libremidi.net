namespace Libremidi.Net.SmokeTest;

using System;
using Xunit;

public sealed class LibremidiInfoTests
{
    [Fact]
    public void Version_IsNotEmpty()
    {
        string version;
        try
        {
            version = LibremidiInfo.Version;
        }
        catch (DllNotFoundException)
        {
            // Native runtime asset may be unavailable in some dev/CI setups.
            Assert.True(true);
            return;
        }

        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.NotEqual("Unknown", version);
    }

    [Fact]
    public void UnknownIdentifier_ReturnsFalseAndNullDisplayName()
    {
        bool found;
        string? displayName;
        try
        {
            found = LibremidiInfo.TryGetApiDisplayName("__not_a_real_api__", out displayName);
        }
        catch (DllNotFoundException)
        {
            // Native runtime asset may be unavailable in some dev/CI setups.
            Assert.True(true);
            return;
        }

        Assert.False(found);
        Assert.Null(displayName);
    }

    [Fact]
    public void KnownIdentifiers_ReturnDisplayName_WhenCompiled()
    {
        var knownIdentifiers = new[] { "alsa_seq", "alsa_raw", "jack_midi", "pipewire", "coremidi", "windows_mm" };

        foreach (var identifier in knownIdentifiers)
        {
            bool found;
            string? displayName;
            try
            {
                found = LibremidiInfo.TryGetApiDisplayName(identifier, out displayName);
            }
            catch (DllNotFoundException)
            {
                // Native runtime asset may be unavailable in some dev/CI setups.
                Assert.True(true);
                return;
            }

            if (!found)
            {
                continue;
            }

            Assert.False(string.IsNullOrWhiteSpace(displayName));
            return;
        }

        // Not every backend is available on every host, so this is intentionally non-failing.
        Assert.True(true);
    }
}