using Ae.Ntp.Protocol;
using ro.bocan.sntpclient;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;

var sntp = new SNTPClient();
sntp.Connect("uk.pool.ntp.org", 5000);

var p0 = new NtpPacket();
int offset = 0;
p0.ReadBytes(sntp.SNTPData, ref offset);

Debug.Assert(sntp.PollInterval == p0.PollIntervalMarshaled);
Debug.Assert(sntp.Precision == p0.PrecisionMarshaled);
Console.WriteLine(p0.ReceiveTimestamp);

//var seconds = (p0.RootDelay >> 16) & 0xFFFF;
//var fraction = p0.RootDelay & 0xFFFF;
//var fraction_seconds = (double)fraction / 0xFFFF;
//var f = fraction_seconds * 1000;

//var t1 = Convert.ToString(p0.RootDelay, 2);

var delay = p0.RootDelay;

var m = new NtpShort { Marshaled = p0.RootDelay.Marshaled };

var n = new NtpTimestamp { Marshaled = p0.ReceiveTimestamp.Marshaled };

var frac = p0.RootDelay.Seconds + (p0.RootDelay.Fraction / (double)ushort.MaxValue * 1000);

var size = Marshal.SizeOf(typeof(NtpPacket));

var client = new UdpClient("uk.pool.ntp.org", 123);

var query = new byte[48];
query[0] = 0x1b;

var queryPacket = new NtpPacket();
var qp = 0;
queryPacket.ReadBytes(query, ref qp);

//var test1 = FromBinaryReader<NtpPacket>(query);

var p1 = new NtpPacket
{
    Mode = NtpMode.Client,
    VersionNumber = 3
};

var q1 = new byte[48];

var t = 0;
p1.WriteBytes(q1, ref t);
await client.SendAsync(q1);

var result = await client.ReceiveAsync();

var packet = new NtpPacket();
var o = 0;
packet.ReadBytes(result.Buffer, ref o);

//var test = FromBinaryReader<NtpPacket>(result.Buffer);

Console.ReadLine();