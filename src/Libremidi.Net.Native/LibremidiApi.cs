namespace Libremidi.Net.Native;

internal enum LibremidiApi
{
    // ...keep values in sync with external/libremidi/include/libremidi/api-c.h
    Unspecified = 0x0, /*!< Search for a working compiled API. */

    // MIDI 1.0 APIs
    Coremidi = 0x1, /*!< macOS CoreMidi API. */
    AlsaSeq, /*!< Linux ALSA Sequencer API. */
    AlsaRaw, /*!< Linux Raw ALSA API. */
    JackMidi, /*!< JACK Low-Latency MIDI Server API. */
    WindowsMm, /*!< Microsoft Multimedia MIDI API. */
    WindowsUwp, /*!< Microsoft WinRT MIDI API. */
    Webmidi, /*!< Web MIDI API through Emscripten */
    Pipewire, /*!< PipeWire */
    Keyboard, /*!< Computer keyboard input */
    Network, /*!< MIDI over IP */
    AndroidAmidi, /*!< Android AMidi API */
    Kdmapi, /*!< OmniMIDI KDMAPI (Windows) */
    RawIo, /*!< User-provided raw byte I/O (serial, SPI, USB, etc.) */

    // MIDI 2.0 APIs
    AlsaRawUmp = 0x1000, /*!< Raw ALSA API for MIDI 2.0 */
    AlsaSeqUmp, /*!< Linux ALSA Sequencer API for MIDI 2.0 */
    CoremidiUmp, /*!< macOS CoreMidi API for MIDI 2.0. Requires macOS 11+ */
    WindowsMidiServices, /*!< Windows API for MIDI 2.0. Requires Windows 11 */
    KeyboardUmp, /*!< Computer keyboard input */
    NetworkUmp, /*!< MIDI2 over IP */
    JackUmp, /*!< MIDI2 over JACK, type "32 bit raw UMP". Requires PipeWire v1.4+. */
    PipewireUmp, /*!< MIDI2 over PipeWire. Requires v1.4+. */
    RawIoUmp, /*!< User-provided raw UMP I/O (serial, SPI, USB, etc.) */

    Dummy = 0xFFFF /*!< A compilable but non-functional API. */
}