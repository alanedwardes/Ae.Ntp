using Ae.Ntp.Protocol;

namespace Ae.Ntp.Client
{
    public sealed class NtpSystemTimeClient : INtpClient
    {
        public void Dispose()
        {
        }

        public Task<NtpPacket> Query(NtpPacket query, CancellationToken token = default)
        {
            var test = new NtpPacket
            {
                VersionNumber = 3,
                Stratum = 3,
                Mode = NtpMode.Server,
                ReceiveTimestamp = new NtpTimestamp { Marshaled = DateTime.UtcNow },
                TransmitTimestamp = new NtpTimestamp { Marshaled = DateTime.UtcNow },
                ReferenceTimestamp = new NtpTimestamp { Marshaled = DateTime.UtcNow },
                OriginateTimestamp = new NtpTimestamp { Marshaled = DateTime.UtcNow }
            };

            return Task.FromResult(test);
        }
    }
}
