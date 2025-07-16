
using System.Collections;
using System.Net;

namespace IPMarcher.Utils;

public class IPRange(IPAddress start, IPAddress end) : IEnumerable<IPAddress>
{
    private readonly IPAddress _start = start;
    private readonly IPAddress _end = end;

    public IEnumerator<IPAddress> GetEnumerator()
    {
        return new IPRangeEnumerator(_start, _end);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

