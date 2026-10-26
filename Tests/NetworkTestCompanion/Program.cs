// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NetworkTestCompanion;

const int DefaultControlPort = 11000;
int[] WellKnownTcpPorts = [DefaultControlPort, 7, 8, 9, 10, 80, 8080, 7010, 7011, 7012, 7013, 7014, 7015];
int[] WellKnownUdpPorts = [7, 8, 9];

// Argument parsing
var cliArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
bool setupFirewall = cliArgs.Contains("--setup-firewall");
bool removeFirewall = cliArgs.Contains("--remove-firewall");
int controlPort = DefaultControlPort;

// Listener binds to Any so it accepts both LAN (MCU) and loopback (emulator) connections.
// --ip overrides the bind address AND the reported LAN IP.
IPAddress bindAddress = IPAddress.Any;
var lanAddresses = RankLanAddresses();
IPAddress lanIp = lanAddresses.Count > 0 ? lanAddresses[0].Address : IPAddress.Loopback;
bool ipFromArgs = false;

for (int i = 0; i < cliArgs.Length; i++)
{
    if (cliArgs[i] == "--control-port" && i + 1 < cliArgs.Length)
    {
        controlPort = int.Parse(cliArgs[++i]);
    }
    else if (cliArgs[i] == "--ip" && i + 1 < cliArgs.Length)
    {
        bindAddress = IPAddress.Parse(cliArgs[++i]);
        lanIp = bindAddress;
        ipFromArgs = true;
    }
}

// Firewall helpers
if (setupFirewall)
{
    FirewallHelper.Setup(WellKnownTcpPorts, WellKnownUdpPorts);
    return;
}

if (removeFirewall)
{
    FirewallHelper.Remove(WellKnownTcpPorts, WellKnownUdpPorts);
    return;
}

// Normal run: start control channel
Console.WriteLine($".NET nanoFramework Network Test Companion");
Console.WriteLine($"  LAN IP      : {lanIp}  (use this in TestConfiguration.CompanionIP for real hardware)");

if (!ipFromArgs && lanAddresses.Count > 1)
{
    Console.WriteLine("  Other local addresses (if the MCU is on one of these networks, restart with --ip <address>):");

    foreach (var (address, iface) in lanAddresses.Skip(1))
    {
        Console.WriteLine($"                {address,-15} {iface}");
    }
}

Console.WriteLine($"  Control port: {controlPort}");
Console.WriteLine($"  Press Ctrl+C to stop.");
Console.WriteLine();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

using var commandServer = new CommandServer(bindAddress, controlPort);
commandServer.Start();

Console.WriteLine($"READY ip={lanIp} port={controlPort}");

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException) { }

Console.WriteLine("Shutting down.");

// Picks the address an MCU on the same LAN can reach. Trying to skip unreliable interfaces based on
// adapter type and name aren't reliable
static List<(IPAddress Address, string Interface)> RankLanAddresses()
{
    var candidates = new List<(IPAddress Address, string Interface, int Score)>();

    foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (iface.OperationalStatus != OperationalStatus.Up
            || iface.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
        {
            continue;
        }

        var props = iface.GetIPProperties();
        bool hasGateway = props.GatewayAddresses.Any(g =>
            g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any));

        foreach (var addr in props.UnicastAddresses)
        {
            if (addr.Address.AddressFamily != AddressFamily.InterNetwork
                || addr.PrefixLength >= 32)
            {
                continue;
            }

            // link-local is kept for an MCU cabled straight to the PC (no DHCP), but ranks below any LAN address
            int score = IsLinkLocal(addr.Address)
                ? -1
                : (hasGateway ? 2 : 0) + (IsPrivate(addr.Address) ? 1 : 0);
            candidates.Add((addr.Address, iface.Name, score));
        }
    }

    // stable sort: keeps OS interface order among equal scores
    return candidates
        .OrderByDescending(c => c.Score)
        .Select(c => (c.Address, c.Interface))
        .ToList();
}

static bool IsLinkLocal(IPAddress address)
{
    var b = address.GetAddressBytes();
    return b[0] == 169 && b[1] == 254;
}

static bool IsPrivate(IPAddress address)
{
    var b = address.GetAddressBytes();
    return b[0] == 10
        || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
        || (b[0] == 192 && b[1] == 168);
}
