using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Net.Sockets;
using IPMarcher;
using IPMarcher.Utils;


Option<int> portOption = new("--port", "-p")
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

Option<IPAddress> endIp = new("--ip-end", "-s")
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
int ports = parseResult.GetValue(portOption);
int timeout = parseResult.GetValue(timeoutOption);

var range = new IPRange(start, end);

List<HostInformation> results = [];
foreach (var ip in range)
{
    var port = await IsPortOpenAsync(ip, ports, TimeSpan.FromSeconds(timeout));
    var hostname = (await GetHostNameAsync(ip)) ?? string.Empty;

    List<int> openPorts = [];
    if (port)
    {
        openPorts.Add(22);
    }

    results.Add(new HostInformation(ip, hostname, [.. openPorts]));
}

foreach (var result in results)
{
    Console.WriteLine($"{result.IpAddress}\t{result.Hostname}\t{result.OpenPorts}");
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

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
    if (!IPAddress.TryParse(result.Tokens.Single().Value, out IPAddress iPAddress))
    {
        return iPAddress;
    }
    else
    {
        result.AddError($"{result.Tokens.Single().Value} is not a valid IP Adress");
    }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

    return IPAddress.None;
}