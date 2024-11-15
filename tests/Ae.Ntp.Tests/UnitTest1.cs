using Ae.Ntp.Client;
using Ae.Ntp.Protocol;
using System.Net;

namespace Ae.Ntp.Tests;

public class UnitTest1
{
    [Fact]
    public async void Test1()
    {
        var client = new NtpUdpClient(new NtpUdpClientOptions { Endpoint = new DnsEndPoint("time.windows.com", 123) });

        var packet = new NtpPacket { Mode = NtpMode.Client, VersionNumber = 3 };

        var response = await client.Query(packet, CancellationToken.None);
    }
}