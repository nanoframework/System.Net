// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace NetworkTestCompanion;

internal static class FirewallHelper
{
    private const string RulePrefix = "nF-TestCompanion";

    internal static void Setup(int[] tcpPorts, int[] udpPorts)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SetupWindows(tcpPorts, udpPorts);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            PrintMacOsGuidance();
        }
        else
        {
            SetupLinux(tcpPorts, udpPorts);
        }
    }

    internal static void Remove(int[] tcpPorts, int[] udpPorts, int[] legacyTcpPorts)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RemoveWindows(tcpPorts, udpPorts, legacyTcpPorts);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("macOS: no rules were added automatically - nothing to remove.");
        }
        else
        {
            Console.WriteLine("Linux: remove any ufw rules you added manually with 'sudo ufw delete allow <port>/tcp'.");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void SetupWindows(int[] tcpPorts, int[] udpPorts)
    {
        if (!IsElevatedWindows())
        {
            Console.Error.WriteLine("ERROR: --setup-firewall requires administrator privileges on Windows.");
            Console.Error.WriteLine("Re-run this command from an elevated prompt:");
            Console.Error.WriteLine($"  runas /user:Administrator \"{Environment.ProcessPath}\" --setup-firewall");
            Environment.Exit(1);
        }

        foreach (var port in tcpPorts)
        {
            RunNetsh($"advfirewall firewall add rule name=\"{RulePrefix}-TCP-{port}\" dir=in action=allow protocol=TCP localport={port}");
        }

        foreach (var port in udpPorts)
        {
            RunNetsh($"advfirewall firewall add rule name=\"{RulePrefix}-UDP-{port}\" dir=in action=allow protocol=UDP localport={port}");
        }

        Console.WriteLine($"Windows firewall rules added for TCP ports [{string.Join(", ", tcpPorts)}] and UDP ports [{string.Join(", ", udpPorts)}].");
        Console.WriteLine($"Remove them later with: --remove-firewall");
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void RemoveWindows(int[] tcpPorts, int[] udpPorts, int[] legacyTcpPorts)
    {
        if (!IsElevatedWindows())
        {
            Console.Error.WriteLine("ERROR: --remove-firewall requires administrator privileges on Windows.");
            Environment.Exit(1);
        }

        foreach (var port in tcpPorts)
        {
            RunNetsh($"advfirewall firewall delete rule name=\"{RulePrefix}-TCP-{port}\"");
        }

        foreach (var port in udpPorts)
        {
            RunNetsh($"advfirewall firewall delete rule name=\"{RulePrefix}-UDP-{port}\"");
        }

        // rules from older setups may not exist, so a failed delete is only an error when the rule is actually there
        foreach (var port in legacyTcpPorts)
        {
            var name = $"{RulePrefix}-TCP-{port}";

            if (!TryRunNetsh($"advfirewall firewall delete rule name=\"{name}\"", out var error)
                && TryRunNetsh($"advfirewall firewall show rule name=\"{name}\"", out _))
            {
                throw new InvalidOperationException(error);
            }
        }

        Console.WriteLine($"Windows firewall rules removed for TCP ports [{string.Join(", ", tcpPorts)}] and UDP ports [{string.Join(", ", udpPorts)}].");
    }

    private static void PrintMacOsGuidance()
    {
        Console.WriteLine("macOS: The Application Firewall will prompt you to Allow or Deny when the companion");
        Console.WriteLine("first starts listening. Click 'Allow' to permit inbound connections from MCU devices.");
        Console.WriteLine("No automated action is needed.");
    }

    private static void SetupLinux(int[] tcpPorts, int[] udpPorts)
    {
        // Print ufw instructions unconditionally: 'ufw status' requires elevated
        // permissions and silently returns no output when run as a normal user,
        // which would suppress necessary guidance even when ufw is active.
        Console.WriteLine("Linux: if ufw is active, run the following commands to open the required ports:");

        foreach (var port in tcpPorts)
        {
            Console.WriteLine($"  sudo ufw allow {port}/tcp");
        }

        foreach (var port in udpPorts)
        {
            Console.WriteLine($"  sudo ufw allow {port}/udp");
        }

        Console.WriteLine("NOTE: for iptables/nftables environments, see Tests/README.md for equivalent rules.");
    }

    private static void RunNetsh(string args)
    {
        if (!TryRunNetsh(args, out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    private static bool TryRunNetsh(string args, out string error)
    {
        var psi = new ProcessStartInfo("netsh", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start netsh.");
        var output = proc.StandardOutput.ReadToEnd();
        var err = proc.StandardError.ReadToEnd().Trim();
        proc.WaitForExit();

        if (proc.ExitCode != 0)
        {
            // netsh reports most failures on stdout
            error = $"netsh failed (exit {proc.ExitCode}): {(err.Length > 0 ? err : output.Trim())}";
            return false;
        }

        error = string.Empty;
        return true;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static bool IsElevatedWindows()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
