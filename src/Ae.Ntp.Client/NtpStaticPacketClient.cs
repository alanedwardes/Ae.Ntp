using Ae.Ntp.Protocol;

namespace Ae.Ntp.Client
{
    public sealed class NtpStaticPacketClient : INtpClient
    {
        private readonly INtpTimeSource _timeSource;

        public NtpStaticPacketClient(INtpTimeSource timeSource)
        {
            _timeSource = timeSource;
        }

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
                ReferenceTimestamp = new NtpTimestamp { Marshaled = _timeSource.Now },
                OriginateTimestamp = query.TransmitTimestamp
            };

            return Task.FromResult(test);
        }
    }
}
