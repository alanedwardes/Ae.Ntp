using Ae.Ntp.Protocol;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Ae.Ntp.Client
{
    /// <summary>
    /// A client which logs metrics aganst NTP responses.
    /// </summary>
    public sealed class NtpMetricsClient : INtpClient
    {
        /// <summary>
        /// The name of the metrics meter.
        /// </summary>
        public static readonly string MeterName = "Ae.Ntp.Client.NtpMetricsClient";

        /// <summary>
        /// The name of the successful response counter.
        /// </summary>
        public static readonly string SuccessCounterName = "Success";

        /// <summary>
        /// The name of the exception error response counter.
        /// </summary>
        public static readonly string ExceptionErrorCounterName = "ExceptionError";

        private static readonly Meter _meter = new Meter(MeterName);
        private static readonly Counter<int> _successCounter = _meter.CreateCounter<int>(SuccessCounterName);
        private static readonly Counter<int> _exceptionCounter = _meter.CreateCounter<int>(ExceptionErrorCounterName);

        private readonly INtpClient _ntpClient;

        /// <summary>
        /// Construct a new <see cref="NtpMetricsClient"/> using the specified <see cref="INtpClient"/>.
        /// </summary>
        public NtpMetricsClient(INtpClient ntpClient) => _ntpClient = ntpClient;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public async Task<NtpPacket> Query(NtpPacket query, CancellationToken token = default)
        {
            var sw = Stopwatch.StartNew();

            var queryTag = new KeyValuePair<string, object>("Query", query);

            NtpPacket answer;
            try
            {
                answer = await _ntpClient.Query(query, token);
            }
            catch
            {
                _exceptionCounter.Add(1, queryTag);
                throw;
            }
            finally
            {
                sw.Stop();
            }

            var answerTag = new KeyValuePair<string, object>("Answer", answer);
            var elapsedTag = new KeyValuePair<string, object>("Elapsed", sw.Elapsed);
            _successCounter.Add(1, queryTag, answerTag, elapsedTag);
            return answer;
        }
    }
}
