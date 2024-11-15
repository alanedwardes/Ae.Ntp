namespace Ae.Ntp.Protocol
{
    /// <summary>
    /// Represents a client capable of returning a NTP answer for a query.
    /// </summary>
    public interface INtpClient : IDisposable
    {
        /// <summary>
        /// Return an answer for the specified NTP query.
        /// </summary>
        /// <param name="query">The NTP query to run, see <see cref="NtpQueryFactory"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use to cancel the operation.</param>
        /// <returns>The <see cref="NtpPacket"/> result.</returns>
        Task<NtpPacket> Query(NtpPacket query, CancellationToken token = default);
    }
}