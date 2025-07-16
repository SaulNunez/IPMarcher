using System.Collections;
using System.Net;

namespace IPMarcher.Utils;

public class IPRangeEnumerator : IEnumerator<IPAddress>
{
    private readonly uint _start;
    private readonly uint _end;
    private uint _current;
    private bool _started = false;

    public IPRangeEnumerator(IPAddress startIP, IPAddress endIP)
    {
        if (startIP.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
            endIP.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            throw new ArgumentException("Only IPv4 addresses are supported.");
        }

        _start = IPToUInt32(startIP);
        _end = IPToUInt32(endIP);

        if (_start > _end)
        {
            throw new ArgumentException("Start IP must be less than or equal to end IP.");
        }

        _current = _start - 1; // Will be incremented on first MoveNext()
    }

    public IPAddress Current => UInt32ToIP(_current);

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (!_started)
        {
            _started = true;
            _current = _start;
            return true;
        }

        if (_current < _end)
        {
            _current++;
            return true;
        }

        return false;
    }

    public void Reset()
    {
        _current = _start - 1;
        _started = false;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }

    private static uint IPToUInt32(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToUInt32(bytes, 0);
    }

    private static IPAddress UInt32ToIP(uint ipInt)
    {
        byte[] bytes = BitConverter.GetBytes(ipInt);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return new IPAddress(bytes);
    }
}
