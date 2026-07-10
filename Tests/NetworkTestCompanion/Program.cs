// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NetworkTestCompanion;

const int DefaultControlPort = 11000;
int[] WellKnownTcpPorts = [DefaultControlPort, 7, 8, 9, 10, 80, 8080];
int[] WellKnownUdpPorts = [7, 8, 9];

// Argument parsing
var cliArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
bool setupFirewall = cliArgs.Contains("--setup-firewall");
bool removeFirewall = cliArgs.Contains("--remove-firewall");
int controlPort = DefaultControlPort;

// Listener binds to Any so it accepts both LAN (MCU) and loopback (emulator) connections.
// --ip overrides the bind address AND the reported LAN IP.
IPAddress bindAddress = IPAddress.Any;
IPAddress lanIp = ResolveLanIp();

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

static IPAddress ResolveLanIp()
{
    foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (iface.OperationalStatus != OperationalStatus.Up)
        {
            continue;
        }

        if (iface.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
        {
            foreach (var addr in iface.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr.Address;
                }
            }
        }
    }

    return IPAddress.Loopback;
}
