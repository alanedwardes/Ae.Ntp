using Ae.Ntp.Client;
using Ae.Ntp.Protocol;
using Humanizer;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;

namespace Ae.Ntp.Console
{
    public sealed class Startup
    {
        private static readonly string MINIMAL_CSS =
            "<style>" +
            "form{display:inline;}" +
            "body{background-color:Canvas;color:CanvasText;color-scheme:light dark;font-family:sans-serif;}" +
            "</style>";

        public void Configure(IApplicationBuilder app)
        {
            MeterListener meterListener = new()
            {
                InstrumentPublished = (instrument, listener) =>
                {
                    if (instrument.Meter.Name.StartsWith("Ae.Ntp"))
                    {
                        listener.EnableMeasurementEvents(instrument);
                    }
                }
            };
            meterListener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
            meterListener.Start();

            app.Run(async context =>
            {
                async Task WriteTable(DataTable table)
                {
                    await context.Response.WriteAsync("<table>");
                    await context.Response.WriteAsync("<thead>");
                    await context.Response.WriteAsync("<tr>");
                    foreach (DataColumn heading in table.Columns)
                    {
                        await context.Response.WriteAsync($"<th>{heading.ColumnName}</th>");
                    }
                    await context.Response.WriteAsync("</tr>");
                    await context.Response.WriteAsync("</thead>");

                    await context.Response.WriteAsync("<tbody>");
                    foreach (DataRow row in table.Rows)
                    {
                        await context.Response.WriteAsync("<tr>");
                        foreach (var item in row.ItemArray)
                        {
                            await context.Response.WriteAsync($"<td>{item}</td>");
                        }
                        await context.Response.WriteAsync("</tr>");
                    }
                    await context.Response.WriteAsync("</tbody>");
                    await context.Response.WriteAsync("</table>");
                }

                var pageLimit = 20;
                if (context.Request.Query.ContainsKey("limit"))
                {
                    _ = int.TryParse(context.Request.Query["limit"], out pageLimit);
                }

                async Task GroupToTable(IEnumerable<IGrouping<string?, NtpStatistic>> groups, params string[] headings)
                {
                    var table = new DataTable();

                    foreach (var heading in headings)
                    {
                        table.Columns.Add(heading);
                    }

                    table.Columns.Add("Percentage");

                    var itemCounts = groups.Select(x => KeyValuePair.Create(x.Key, x.Count())).OrderByDescending(x => x.Value).ToList();
                    var totalCount = itemCounts.Sum(x => x.Value);

                    int CalculatePercentage(int count) => (int)(count / (double)totalCount * (double)100d);

                    foreach (var group in itemCounts.Take(pageLimit))
                    {
                        table.Rows.Add(group.Key, group.Value, CalculatePercentage(group.Value) + "%");
                    }

                    var remaining = itemCounts.Skip(pageLimit).Sum(x => x.Value);
                    if (remaining > 0)
                    {
                        table.Rows.Add("Other", remaining, CalculatePercentage(remaining) + "%");
                    }

                    await WriteTable(table);
                }

                IEnumerable<NtpStatistic> query = _queries.OrderByDescending(x => x.Created);

                var filteredQueries = query.ToArray();

                string SenderFilter(NtpStatistic ntpStatistic)
                {
                    return $"<a href=\"{CreateQueryString("sender", ntpStatistic.Sender)}\">{ntpStatistic.Sender}</a>";
                }

                string ServerFilter(NtpStatistic ntpStatistic)
                {
                    return $"<a href=\"{CreateQueryString("server", ntpStatistic.Query.Tags["Server"])}\">{ntpStatistic.Query.Tags["Server"]}</a>";
                }

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(MINIMAL_CSS);

                await context.Response.WriteAsync($"<h2>Current Time</h2>");
                await context.Response.WriteAsync($"<p>{DateTime.UtcNow:O}</p>");

                string CreateQueryString(string name, object? value)
                {
                    IDictionary<string, string?> filters = context.Request.Query.ToDictionary(x => x.Key, x => (string?)x.Value.ToString());
                    var valueString = value?.ToString();
                    if (valueString != null)
                    {
                        filters[name] = valueString;
                    }
                    return QueryHelpers.AddQueryString(context.Request.Path, filters);
                }

                await context.Response.WriteAsync($"<h2>Top Clients</h2>");
                await context.Response.WriteAsync($"<p>Top NTP clients.</p>");
                await GroupToTable(filteredQueries.GroupBy(SenderFilter), "Client Address", "Hits");

                await context.Response.WriteAsync($"<h2>Top Servers</h2>");
                await context.Response.WriteAsync($"<p>Top servers used.</p>");
                await GroupToTable(filteredQueries.GroupBy(ServerFilter), "Server", "Hits");

                var recentQueries = new DataTable { Columns = { "Timestamp", "Sender", "Drift", "Duration (microseconds)" } };
                foreach (var ntpStatistic in filteredQueries.Take(pageLimit))
                {
                    recentQueries.Rows.Add(ntpStatistic.Created, SenderFilter(ntpStatistic), (ntpStatistic.Created - ntpStatistic.Query.TransmitTimestamp.Marshaled).Humanize(), ntpStatistic.Elapsed?.TotalMicroseconds.ToString("F"));
                }

                await context.Response.WriteAsync($"<h2>Recent Queries</h2>");
                await context.Response.WriteAsync($"<p>50 most recent queries / answers.</p>");
                await WriteTable(recentQueries);
            });
        }

        public sealed class NtpStatistic
        {
            public NtpPacket Query { get; set; }
            public NtpPacket Answer { get; set; }
            public TimeSpan? Elapsed { get; set; }
            public IPAddress? Sender { get; set; }
            public DateTime Created { get; set; }
        }

        private readonly ConcurrentQueue<NtpStatistic> _queries = new();

        private void OnMeasurementRecorded(Instrument instrument, int measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
        {
            static TObject? GetObjectFromTags<TObject>(ReadOnlySpan<KeyValuePair<string, object?>> _tags, string name)
            {
                foreach (var tag in _tags)
                {
                    if (tag.Key == name)
                    {
                        return (TObject?)tag.Value;
                    }
                }

                return default(TObject);
            }

            if (instrument.Meter.Name == NtpMetricsClient.MeterName)
            {
                var query = GetObjectFromTags<NtpPacket>(tags, "Query");
                var answer = GetObjectFromTags<NtpPacket>(tags, "Answer");
                var elapsed = GetObjectFromTags<Stopwatch?>(tags, "Elapsed")?.Elapsed ?? TimeSpan.Zero;
                var sender = query.Tags.TryGetValue("Sender", out var rawEndpoint) && rawEndpoint is not null && rawEndpoint is IPEndPoint endpoint ? endpoint.Address : null;
                if (sender != null)
                {
                    _queries.Enqueue(new NtpStatistic
                    {
                        Query = query,
                        Answer = answer,
                        Elapsed = elapsed,
                        Sender = sender,
                        Created = DateTime.UtcNow - elapsed
                    });

                    if (_queries.Count > 100_000)
                    {
                        _queries.TryDequeue(out var _);
                    }
                }
            }
        }
    }
}