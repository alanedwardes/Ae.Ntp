using Ae.Ntp.Protocol;
using Xunit;

namespace Ae.Ntp.Tests;

public class NtpTimestampTests
{
    [Fact]
    public void TestRandomRoundtrip()
    {
        var random = new Random();

        for (int i = 0; i < 10_000_000; i++)
        {
            var expected = NtpTimestamp.NtpEpoch + TimeSpan.FromSeconds((double)random.Next() * 2);

            var actual = new NtpTimestamp { Marshaled = expected }.Marshaled;

            Assert.Equal(expected, actual);
        }
    }
}