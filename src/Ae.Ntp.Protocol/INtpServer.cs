namespace Ae.Ntp.Protocol
{
    /// <summary>
    /// Represents a server capable of receiving NTP requests.
    /// </summary>
    public interface INtpServer : IDisposable
    {
        /// <summary>
        /// Listen for NTP queries.
        /// </summary>
        /// <param name="token">The <see cref="CancellationToken"/> to use to stop listening.</param>
        /// <returns>A task which will run forever unless cancelled.</returns>
        Task Listen(CancellationToken token = default);
    }
}