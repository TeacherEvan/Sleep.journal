using Microsoft.Extensions.Logging;
using Moq;
using SleepJournal.Services;

namespace SleepJournal.Tests.Services;

/// <summary>
/// Unit tests for AudioService. Pins the PCM/WAV synthesis contract so the
/// sound payload is deterministic and behaviour survives refactors.
/// </summary>
public class AudioServiceTests
{
    private static AudioService BuildService()
    {
        var logger = new Mock<ILogger<AudioService>>();
        return new AudioService(logger.Object);
    }

    [Fact]
    public void GenerateWav_ReturnsValidRiffWavHeader()
    {
        var svc = BuildService();
        var wav = svc.GenerateWav(440, 100, 0.5f);

        // RIFF + WAVE markers
        wav[0].Should().Be((byte)'R');
        wav[1].Should().Be((byte)'I');
        wav[2].Should().Be((byte)'F');
        wav[3].Should().Be((byte)'F');
        wav[8].Should().Be((byte)'W');
        wav[9].Should().Be((byte)'A');
        wav[10].Should().Be((byte)'V');
        wav[11].Should().Be((byte)'E');
        // fmt + data sub-chunk markers
        wav[12].Should().Be((byte)'f');
        wav[36].Should().Be((byte)'d');
    }

    [Fact]
    public void GenerateWav_HeaderLengthIs44Bytes()
    {
        var svc = BuildService();
        var wav = svc.GenerateWav(440, 100, 0.5f);

        // 100ms @ 44100Hz = 4410 samples * 2 bytes = 8820 data bytes + 44 header
        wav.Length.Should().Be(44 + 4410 * 2);
    }

    [Fact]
    public void GenerateWav_SampleCountMatchesDuration()
    {
        var svc = BuildService();
        var wav = svc.GenerateWav(440, 250, 0.5f);

        // 250ms @ 44100Hz = 11025 samples * 2 bytes
        wav.Length.Should().Be(44 + 11025 * 2);
    }

    [Fact]
    public void GenerateWav_VolumeScalesPeakAmplitude()
    {
        var svc = BuildService();
        var quiet = svc.GenerateWav(440, 100, 0.01f);
        var loud = svc.GenerateWav(440, 100, 1.0f);

        // Max absolute sample value for the loud signal should exceed the quiet one.
        var quietMax = MaxAbsSample(quiet);
        var loudMax = MaxAbsSample(loud);
        loudMax.Should().BeGreaterThan(quietMax);
    }

    [Fact]
    public void GenerateWav_RejectsNonPositiveFrequency()
    {
        var svc = BuildService();
        var act = () => svc.GenerateWav(0, 100, 0.5f);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateWav_RejectsNegativeDuration()
    {
        var svc = BuildService();
        var act = () => svc.GenerateWav(440, -1, 0.5f);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateWav_RejectsVolumeOutOfRange()
    {
        var svc = BuildService();
        svc.GenerateWav(440, 100, 0.5f).Should().NotBeNull();
        var act1 = () => svc.GenerateWav(440, 100, -0.1f);
        var act2 = () => svc.GenerateWav(440, 100, 1.1f);
        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetVolume_RejectsOutOfRange()
    {
        var svc = BuildService();
        var act1 = () => svc.SetVolume(-0.1f);
        var act2 = () => svc.SetVolume(1.1f);
        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetVolume_UpdatesGetVolume()
    {
        var svc = BuildService();
        svc.SetVolume(0.42f);
        svc.GetVolume().Should().Be(0.42f);
    }

    [Fact]
    public void SetAudioEnabled_TogglesIsAudioEnabled()
    {
        var svc = BuildService();
        svc.IsAudioEnabled.Should().BeTrue();
        svc.SetAudioEnabled(false);
        svc.IsAudioEnabled.Should().BeFalse();
        svc.SetAudioEnabled(true);
        svc.IsAudioEnabled.Should().BeTrue();
    }

    private static int MaxAbsSample(byte[] wav)
    {
        var max = 0;
        for (var i = 44; i + 1 < wav.Length; i += 2)
        {
            var sample = (short)(wav[i] | (wav[i + 1] << 8));
            max = Math.Max(max, Math.Abs(sample));
        }
        return max;
    }
}
