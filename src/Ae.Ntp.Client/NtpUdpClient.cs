using Ae.Ntp.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace Ae.Ntp.Client
{
    /// <summary>
    /// A NTP UDP client implementation.
    /// </summary>
    public sealed class NtpUdpClient : INtpClient
    {
        private readonly ILogger<NtpUdpClient> _logger;
        private readonly NtpUdpClientOptions _options;

        /// <summary>
        /// Construct a new <see cref="NtpUdpClient"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        [ActivatorUtilitiesConstructor]
        public NtpUdpClient(ILogger<NtpUdpClient> logger, IOptions<NtpUdpClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Construct a new <see cref="NtpUdpClient"/>.
        /// </summary>
        /// <param name="options"></param>
        public NtpUdpClient(NtpUdpClientOptions options)
            : this(NullLogger<NtpUdpClient>.Instance, Options.Create(options))
        {
        }

        /// <inheritdoc/>
        public async Task<NtpPacket> Query(NtpPacket query, CancellationToken token)
        {
            var buffer = NtpByteExtensions.AllocatePinnedNetworkBuffer();
            var sendOffset = 0;
            query.WriteBytes(buffer, ref sendOffset);

            var sendBuffer = buffer.Slice(0, sendOffset);

            IPEndPoint endpoint;
            if (_options.Endpoint is DnsEndPoint dnsEndpoint)
            {
                var addresses = await Dns.GetHostAddressesAsync(dnsEndpoint.Host, token);
                endpoint = new IPEndPoint(addresses[0], dnsEndpoint.Port);
            }
            else if (_options.Endpoint is IPEndPoint ipEndpoint)
            {
                endpoint = ipEndpoint;
            }
            else
            {
                throw new NotImplementedException($"Unsupported endpoint type: {_options.Endpoint}");
            }

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            await socket.SendToAsync(sendBuffer, SocketFlags.None, endpoint, token);

            var received = await socket.ReceiveFromAsync(buffer, endpoint, token);

            var offset = 0;
            var answerBuffer = buffer.Slice(offset, received.ReceivedBytes);

            var answer = NtpByteExtensions.FromBytes<NtpPacket>(answerBuffer);
            answer.Tags.Add("Clock", ToString());
            return answer;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"udp://{_options.Endpoint}/";
    }
}