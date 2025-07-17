using System.Net;

namespace IPMarcher;

public record HostInformation(IPAddress IpAddress, string Hostname, int[] OpenPorts, bool IsOnline);