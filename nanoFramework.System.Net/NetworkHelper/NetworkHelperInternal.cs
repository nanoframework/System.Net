//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace nanoFramework.Networking
{
    /// <summary>
    /// Network helper class providing helper methods to assist on connecting to a network.
    /// </summary>
    internal static class NetworkHelperInternal
    {
        /// <summary>
        /// Checks if there is an IPAddress assigned to the specified network interface.
        /// </summary>
        public static bool CheckIP(NetworkInterfaceType interfaceType, IPConfiguration _ipConfiguration)
        {
            Debug.WriteLine($"Checking for IP in interface {interfaceType}");

            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in nis)
            {
                if (networkInterface.NetworkInterfaceType == interfaceType
                    && networkInterface.IPv4Address[0] != '0')
                {
                    if (_ipConfiguration != null && _ipConfiguration.IPAddress != networkInterface.IPv4Address)
                    {
                        return false;
                    }

                    Debug.WriteLine($"We have an IP: {networkInterface.IPv4Address}");

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks and waits until a valid DateTime is set on the system.
        /// </summary>
        public static void WaitForValidDateTime()
        {
            // if SNTP is available and enabled on target device this can be skipped because we should have a valid date & time
            while (DateTime.UtcNow.Year < 2021)
            {
                Debug.WriteLine("Checking for valid date time...");

                // wait for valid date & time
                Thread.Sleep(1000);
            }
        }

        public static void InternalSetupHelper(
            NetworkInterface[] nis,
            NetworkInterfaceType targetNetworkInterface,
            IPConfiguration ipConfiguration)
        {

            foreach (var networkInterface in nis)
            {
                // target only the working interface type
                if (networkInterface.NetworkInterfaceType == targetNetworkInterface)
                {
                    if (ipConfiguration != null)
                    {
                        Debug.WriteLine("Setting up static IP v4 configuration");

                        // setup static IPv4
                        networkInterface.EnableStaticIPv4(
                        ipConfiguration.IPAddress,
                        ipConfiguration.IPSubnetMask,
                        ipConfiguration.IPGatewayAddress);

                        // check if static DNS is to be configured
                        if (ipConfiguration.IPDns != null
                            && ipConfiguration.IPDns.Length > 0)
                        {
                            networkInterface.EnableStaticIPv4Dns(ipConfiguration.IPDns);
                        }
                        else
                        {
                            networkInterface.EnableAutomaticDns();
                        }
                    }
                    else
                    {
                        if (!networkInterface.IsDhcpEnabled)
                        {
                            networkInterface.EnableDhcp();
                        }

                        networkInterface.EnableAutomaticDns();
                    }
                }
            }
        }
    }
}
