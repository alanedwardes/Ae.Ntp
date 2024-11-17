using Ae.Ntp.Protocol;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Ae.Ntp.Tests;

public class NtpShortTests
{
    [Fact]
    public void TestRoundTripFractionalSeconds()
    {
        var random = new Random();

        for (ushort i = 0; i < ushort.MaxValue; i++)
        {
            var expected = TimeSpan.FromMilliseconds(i * NtpShort.Segment);
            var actual = new NtpShort { Fraction = i };
            Assert.Equal(expected, actual.Marshaled);
        }
    }

    [Fact]
    public void TestRoundTripSeconds()
    {
        var random = new Random();

        for (ushort i = 0; i < ushort.MaxValue; i++)
        {
            var expected = TimeSpan.FromSeconds(i);
            var actual = new NtpShort { Seconds = i };
            Assert.Equal(expected, actual.Marshaled);
        }
    }

    [Fact]
    public void TestRandomRoundtrip()
    {
        var random = new Random();

        for (int i = 0; i < 10_000_000; i++)
        {
            var expected = new TimeSpan(0, 0, 0, random.Next(0, ushort.MaxValue), random.Next(0, 1000));

            var actual = new NtpShort { Marshaled = expected };

            Assert.Equal(expected.TotalMilliseconds, actual.Marshaled.TotalMilliseconds, 1);
        }
    }
}
