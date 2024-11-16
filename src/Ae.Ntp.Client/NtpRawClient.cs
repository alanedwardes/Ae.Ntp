using Ae.Ntp.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ae.Ntp.Client
{
    /// <inheritdoc/>
    public sealed class NtpRawClient : INtpRawClient
    {
        private readonly ILogger<NtpRawClient> _logger;
        private readonly INtpClient _ntpClient;

        /// <inheritdoc/>
        [ActivatorUtilitiesConstructor]
        public NtpRawClient(ILogger<NtpRawClient> logger, INtpClient ntpClient)
        {
            _logger = logger;
            _ntpClient = ntpClient;
        }

        /// <inheritdoc/>
        public NtpRawClient(INtpClient ntpClient) : this(NullLogger<NtpRawClient>.Instance, ntpClient)
        {
        }

        /// <inheritdoc/>
        public void Dispose() => _ntpClient.Dispose();

        /// <inheritdoc/>
        public async Task<NtpRawClientResponse> Query(Memory<byte> buffer, NtpRawClientRequest request, CancellationToken token = default)
        {
            var queryBuffer = buffer.Slice(0, request.QueryLength);

            NtpPacket query;
            try
            {
                query = NtpByteExtensions.FromBytes<NtpPacket>(queryBuffer);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse incoming query {Bytes}", NtpByteExtensions.ToDebugString(queryBuffer.ToArray()));
                throw;
            }

            //query.Header.Tags.Add("Sender", request.SourceEndpoint);
            //query.Header.Tags.Add("Server", request.ServerName);

            NtpPacket answer;
            try
            {
                answer = await _ntpClient.Query(query, token);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to resolve {Query}", query);
                throw;
            }

            if (!answer.ReceiveTimestamp.Equals(NtpTimestamp.Zero) || !answer.TransmitTimestamp.Equals(NtpTimestamp.Zero))
            {
                throw new InvalidOperationException($"The {nameof(NtpPacket.ReceiveTimestamp)} and {nameof(NtpPacket.TransmitTimestamp)} must be zero.");
            }

            answer.ReceiveTimestamp = new NtpTimestamp { Marshaled = request.ReceiveTime };
            answer.TransmitTimestamp = new NtpTimestamp { Marshaled = DateTime.UtcNow };

            var answerLength = 0;
            try
            {
                answer.WriteBytes(buffer, ref answerLength);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to serialise {Answer} for query {Query}", answer, query);
                throw;
            }

            _logger.LogTrace("Returning {Answer} for query {Query}", answer, query);
            return new NtpRawClientResponse(answerLength, query, answer);
        }
    }
}