using Ae.Ntp.Protocol;
using System.Net.Sockets;
using System.Runtime.InteropServices;

static T FromBinaryReader<T>(byte[] bytes)
{
    // Pin the managed memory while, copy it out the data, then unpin it
    GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
    T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
    handle.Free();

    return theStructure;
}

var size = Marshal.SizeOf(typeof(NtpPacket));

var client = new UdpClient("time.windows.com", 123);

var query = new byte[48];
query[0] = 0x1b;

var test1 = FromBinaryReader<NtpPacket>(query);

await client.SendAsync(query);

var result = await client.ReceiveAsync();

var test = FromBinaryReader<NtpPacket>(result.Buffer);

Console.ReadLine();