using Ae.Ntp.Client;
using Ae.Ntp.Protocol;
using Ae.Ntp.Server;
using Microsoft.Extensions.Options;
using NodaTime;
using System.Net;

namespace Ae.Ntp.Console
{
    class Program
    {
        static void Main(string[] args) => DoWork(args).GetAwaiter().GetResult();

        private static async Task DoWork(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), true)
                .Build();

            var ntpConfiguration = new NtpConfiguration();
            configuration.Bind(ntpConfiguration);

            var services = new ServiceCollection();
            services.AddLogging(x => x.AddConsole());
            services.Configure<NtpUdpServerOptions>(configuration.GetSection("udpServer"));

            IServiceProvider provider = services.BuildServiceProvider();

            var selfLogger = provider.GetRequiredService<ILogger<Program>>();

            selfLogger.LogInformation("Working directory is {WorkingDirectory}", Directory.GetCurrentDirectory());

            INtpClient CreateNtpClient(string source, INtpTimeSource timeSource)
            {
                switch (source)
                {
                    case "static":
                        INtpClient queryClient = ActivatorUtilities.CreateInstance<NtpStaticPacketClient>(provider, timeSource);
                        return new NtpMetricsClient(queryClient);
                    default:
                        throw new NotImplementedException(source);
                }
            }
            
            INtpServer CreateNtpServer(NtpServer server)
            {
                switch (server.Endpoint.Scheme)
                {
                    case "udp":
                        var tz = DateTimeZoneProviders.Tzdb[server.TimeZone];
                        INtpTimeSource timeSource = new NtpFuncTimeSource(() =>
                        {
                            Instant now = SystemClock.Instance.GetCurrentInstant();
                            return now.InZone(tz).ToDateTimeUnspecified();
                        });
                        INtpPacketProcessor rawClient = ActivatorUtilities.CreateInstance<NtpPacketProcessor>(provider, CreateNtpClient(server.Source, timeSource), timeSource);
                        var serverConfig = new NtpUdpServerOptions
                        {
                            Endpoint = new IPEndPoint(IPAddress.Parse(server.Endpoint.Host), server.Endpoint.Port)
                        };
                        return ActivatorUtilities.CreateInstance<NtpUdpServer>(provider, rawClient, Options.Create(serverConfig));
                    default:
                        throw new NotImplementedException(server.Endpoint.ToString());
                }
            }

            var servers = new List<INtpServer>();
            foreach (var server in ntpConfiguration.Servers)
            {
                servers.Add(CreateNtpServer(server));
            }

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                    x.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
                })
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseStartup<Startup>();
                    webHostBuilder.UseConfiguration(configuration.GetSection("statsServer"));
                });

            var tasks = new List<Task> { builder.Build().RunAsync(CancellationToken.None) };
            tasks.AddRange(servers.Select(x => x.Listen(CancellationToken.None)));
            await Task.WhenAll(tasks);
        }
    }
}