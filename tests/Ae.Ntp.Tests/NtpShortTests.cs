using Ae.Ntp.Protocol;
using Xunit;

namespace Ae.Ntp.Tests;

public class NtpShortTests
{
    [Fact(Skip = "NtpShort is broken")]
    public void TestRandomRoundtrip()
    {
        var random = new Random();

        for (int i = 0; i < 10_000_000; i++)
        {
            var expected = TimeSpan.FromMicroseconds(random.Next());

            var actual = new NtpShort { Marshaled = expected }.Marshaled;

            Assert.Equal(expected.TotalMilliseconds, actual.TotalMilliseconds, 0);
        }
    }
}
