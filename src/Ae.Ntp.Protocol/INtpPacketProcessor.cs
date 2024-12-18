﻿using System.Diagnostics;
using System.Net;

namespace Ae.Ntp.Protocol
{
    /// <summary>
    /// Represents a client capable of accepting a contiguous region of memory,
    /// reading the query from it, and writing the answer back into it.
    /// </summary>
    public interface INtpPacketProcessor : IDisposable
    {
        /// <summary>
        /// Reads the number of bytes specified from the buffer, and writes back an answer to the same buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read the query from, and write the answer to.</param>
        /// <param name="request">The raw client request information</param>
        /// <param name="token">The cancellation token to stop the operation.</param>
        /// <returns>The number of bytes written back into the buffer.</returns>
        Task<NtpRawClientResponse> Query(Memory<byte> buffer, NtpRawClientRequest request, CancellationToken token = default);
    }

    /// <summary>
    /// Describes a request from a client.
    /// </summary>
    public readonly struct NtpRawClientRequest
    {
        /// <summary>
        /// Create a new <see cref="NtpRawClientRequest"/>.
        /// </summary>
        /// <param name="queryLength">The length of the query, in bytes.</param>
        /// <param name="sourceEndpoint">The source endpoint for statistical purposes.</param>
        /// <param name="receiveTime">The timestamp showing when the packet was received.</param>
        /// <param name="serverName">The source server for statistical purposes.</param>
        public NtpRawClientRequest(int queryLength, EndPoint sourceEndpoint, Stopwatch receiveTime, string serverName)
        {
            QueryLength = queryLength;
            SourceEndpoint = sourceEndpoint;
            ReceiveTime = receiveTime;
            ServerName = serverName;
        }

        /// <summary>
        /// The length of the query, in bytes.
        /// </summary>
        public readonly int QueryLength;

        /// <summary>
        /// The source endpoint for statistical purposes.
        /// </summary>
        public readonly EndPoint SourceEndpoint;

        /// <summary>
        /// The source server for statistical purposes.
        /// </summary>
        public readonly string ServerName;

        /// <summary>
        /// The time when the packet was receieved.
        /// </summary>
        public readonly Stopwatch ReceiveTime;
    }

    /// <summary>
    /// Describes a response to a client.
    /// </summary>
    public readonly struct NtpRawClientResponse
    {
        /// <summary>
        /// Create a new <see cref="NtpRawClientResponse"/>.
        /// </summary>
        /// <param name="answerLength">The length of the answer, in bytes.</param>
        /// <param name="query">The query message from the client.</param>
        /// <param name="answer">The answer message (already written to the supplied buffer).</param>
        public NtpRawClientResponse(int answerLength, NtpPacket query, NtpPacket answer)
        {
            AnswerLength = answerLength;
            Query = query;
            Answer = answer;
        }

        /// <summary>
        /// The length of the answer, in bytes.
        /// </summary>
        public readonly int AnswerLength;

        /// <summary>
        /// The query message from the client.
        /// </summary>
        public readonly NtpPacket Query;

        /// <summary>
        /// The answer message (already written to the supplied buffer).
        /// </summary>
        public readonly NtpPacket Answer;
    }
}