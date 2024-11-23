using Ae.Ntp.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Ae.Ntp.Server
{
    /// <summary>
    /// Provides a NTP server over UDP.
    /// </summary>
    public sealed class NtpUdpServer : INtpServer
    {
        private static readonly EndPoint _anyEndpoint = new IPEndPoint(IPAddress.Any, 0);
        private readonly NtpUdpServerOptions _options;
        private readonly Socket _socket;
        private readonly ILogger<NtpUdpServer> _logger;
        private readonly INtpPacketProcessor _ntpClient;

        /// <summary>
        /// Construct a new <see cref="NtpUdpServer"/> with a custom logger, options and a <see cref="INtpPacketProcessor"/> to delegate to.
        /// </summary>
        [ActivatorUtilitiesConstructor]
        public NtpUdpServer(ILogger<NtpUdpServer> logger, INtpPacketProcessor ntpClient, IOptions<NtpUdpServerOptions> options)
        {
            _options = options.Value;
            _socket = new Socket(_options.Endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(_options.Endpoint);
            _logger = logger;
            _ntpClient = ntpClient;
        }

        /// <summary>
        /// A convenience constructor where only the <see cref="INtpPacketProcessor"/> is mandated.
        /// </summary>
        public NtpUdpServer(INtpPacketProcessor ntpClient, NtpUdpServerOptions options = null)
            : this(NullLogger<NtpUdpServer>.Instance, ntpClient, Options.Create(options ?? new NtpUdpServerOptions()))
        {
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                _socket.Close();
                _socket.Dispose();
            }
            catch (Exception)
            {
            }
        }

        /// <inheritdoc/>
        public async Task Listen(CancellationToken token)
        {
            token.Register(() => _socket.Close());

            _logger.LogInformation("Now listening on: {Endpoint} (DefaultMaximumDatagramSize: {DefaultMaximumDatagramSize})", "udp://" + _options.Endpoint, _options.DefaultMaximumDatagramSize);

            while (!token.IsCancellationRequested)
            {
                var buffer = NtpByteExtensions.AllocatePinnedNetworkBuffer();

                try
                {
                    var result = await _socket.ReceiveMessageFromAsync(buffer, SocketFlags.None, _anyEndpoint, token);
                    Respond(result.RemoteEndPoint, Stopwatch.StartNew(), buffer, result.ReceivedBytes, token);
                }
                catch (ObjectDisposedException)
                {
                    // Do nothing, server shutting down
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error with incoming connection");
                }
            }
        }

        private async void Respond(EndPoint sender, Stopwatch recieveTime, Memory<byte> buffer, int queryLength, CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();

            var request = new NtpRawClientRequest(queryLength, sender, recieveTime, nameof(NtpUdpServer));

            NtpRawClientResponse response;
            try
            {
                response = await _ntpClient.Query(buffer, request, token);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to run query {QueryBytes} for {RemoteEndPoint}", NtpByteExtensions.ToDebugString(buffer.Slice(0, queryLength)), sender);
                return;
            }

            int answerLength = response.AnswerLength;
            try
            {
                // Send the part of the buffer containing the answer
                await _socket.SendToAsync(buffer.Slice(0, answerLength), SocketFlags.None, sender, token);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to send back answer {AnswerBytes} to {RemoteEndPoint}", NtpByteExtensions.ToDebugString(buffer.Slice(0, answerLength)), sender);
                return;
            }

            _logger.LogTrace("Responded to query from {RemoteEndPoint} in {ResponseTime}", sender, stopwatch.Elapsed.TotalSeconds);
        }
    }
}