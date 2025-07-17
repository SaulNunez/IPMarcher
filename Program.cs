using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using IPMarcher;
using IPMarcher.Utils;


Option<int[]> portOption = new("--port", "-p")
{
    Description = "Port to check.",
    DefaultValueFactory = parseResult => 22,
    Arity = ArgumentArity.OneOrMore,
    AllowMultipleArgumentsPerToken = true
};

Option<IPAddress> startIp = new("--ip-start", "-s")
{
    Description = "Start of IP range to check.",
    DefaultValueFactory = parseResult => new IPAddress([192, 168, 0, 0]),
    CustomParser = ParseIpAddress
};

Option<IPAddress> endIp = new("--ip-end", "-e")
{
    Description = "Start of IP range to check.",
    DefaultValueFactory = parseResult => new IPAddress([192, 168, 0, 254]),
    CustomParser = ParseIpAddress
};

Option<int> timeoutOption = new("--timeout", "-t")
{
    Description = "How much time to wait for a connection to be established.",
    DefaultValueFactory = parseResult => 15
};

RootCommand rootCommand = new("Check for open ports.");
rootCommand.Options.Add(portOption);
rootCommand.Options.Add(startIp);
rootCommand.Options.Add(endIp);
rootCommand.Options.Add(timeoutOption);

ParseResult parseResult = rootCommand.Parse(args);

IPAddress start = parseResult.GetValue(startIp);
IPAddress end = parseResult.GetValue(endIp);
var ports = parseResult.GetValue(portOption);
int timeout = parseResult.GetValue(timeoutOption);

var range = new IPRange(start, end);
var rangeCheck = range.Select(async ip =>
{
    var hostname = string.Empty;
    int[] openPorts = [];
    var hostIsOnline = await IsHostOnlineAsync(ip);
    if (hostIsOnline)
    {
        var results = await Task.WhenAll(ports.Select(async port =>
        {
            var isPortOpen = await IsPortOpenAsync(ip, port, TimeSpan.FromSeconds(timeout));
            return (port, isPortOpen);
        }));
        openPorts = results.Where(x => x.isPortOpen).Select(x => x.port).ToArray();
        hostname = (await GetHostNameAsync(ip)) ?? string.Empty;
    }

    return new HostInformation(ip, hostname, [.. openPorts], hostIsOnline);
}).ToArray();

var results = await Task.WhenAll(rangeCheck);


foreach (var result in results)
{
    var openPortsConcat = string.Join(",", result.OpenPorts);
    Console.WriteLine($"{result.IpAddress}\t{result.Hostname}\t{openPortsConcat}\t{result.IsOnline}");
}

static async Task<bool> IsPortOpenAsync(IPAddress ip, int port, TimeSpan timeout)
{
    using var client = new TcpClient();
    var connectTask = client.ConnectAsync(ip, port);
    var timeoutTask = Task.Delay(timeout);

    var completedTask = await Task.WhenAny(connectTask, timeoutTask);

    return completedTask == connectTask && client.Connected;
}

static async Task<string?> GetHostNameAsync(IPAddress ip)
{
    try
    {
        IPHostEntry entry = await Dns.GetHostEntryAsync(ip);
        return entry.HostName;
    }
    catch
    {
        return null; // Hostname couldn't be resolved
    }
}

static IPAddress ParseIpAddress(ArgumentResult result)
{
    if (!result.Tokens.Any())
    {
        return new IPAddress([192, 168, 0, 0]);
    }

    if (IPAddress.TryParse(result.Tokens.Single().Value, out IPAddress iPAddress))
    {
        return iPAddress;
    }
    else
    {
        result.AddError($"{result.Tokens.Single().Value} is not a valid IP Adress");
    }

    return IPAddress.None;
}

static async Task<bool> IsHostOnlineAsync(IPAddress host, int timeoutMs = 1000)
{
    try
    {
        using var ping = new Ping();
        PingReply reply = await ping.SendPingAsync(host, timeoutMs);
        return reply.Status == IPStatus.Success;
    }
    catch
    {
        return false;
    }
}