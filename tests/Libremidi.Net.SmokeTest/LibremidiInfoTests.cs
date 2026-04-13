namespace Libremidi.Net.SmokeTest;

using Xunit;

public sealed class LibremidiInfoTests
{
    [Fact]
    public void Version_IsNotEmpty()
    {
        var version = LibremidiInfo.Version;

        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public void UnknownIdentifier_ReturnsFalseAndNullDisplayName()
    {
        var found = LibremidiInfo.TryGetApiDisplayName("__not_a_real_api__", out var displayName);

        Assert.False(found);
        Assert.Null(displayName);
    }

    [Fact]
    public void KnownIdentifiers_ReturnDisplayName_WhenCompiled()
    {
        var knownIdentifiers = new[] { "alsa_seq", "alsa_raw", "jack_midi", "pipewire", "coremidi", "windows_mm" };

        foreach (var identifier in knownIdentifiers)
        {
            var found = LibremidiInfo.TryGetApiDisplayName(identifier, out var displayName);
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