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
    public static class NetworkHelper
    {
        private static ManualResetEvent _ipAddressAvailable;
        private static ManualResetEvent _networkReady;

        private static bool _requiresDateTime;
        private static NetworkHelperStatus _networkHelperStatus = NetworkHelperStatus.None;
        private static Exception _helperException;

        // defaulting to Ethernet
        private static NetworkInterfaceType _workingNetworkInterface = NetworkInterfaceType.Ethernet;

        private static IPConfiguration _ipConfiguration;

        /// <summary>
        /// This flag will make sure there is only one and only call to any of the helper methods.
        /// </summary>
        private static bool _helperInstanciated = false;

        /// <summary>
        /// Event signaling that networking it's ready.
        /// </summary>
        /// <remarks>
        /// The conditions for this are setup in the call to <see cref="SetupNetworkHelper"/>. 
        /// It will be a composition of network connected, IpAddress available and valid system <see cref="DateTime"/>.</remarks>
        public static ManualResetEvent NetworkReady => _networkReady;

        /// <summary>
        /// Status of NetworkHelper.
        /// </summary>
        public static NetworkHelperStatus Status => _networkHelperStatus;

        /// <summary>
        /// Exception that occurred when waiting for the network to become ready.
        /// </summary>
        public static Exception HelperException { get => _helperException; internal set => _helperException = value; }

        /// <summary>
        /// This method will setup the network configurations so that the <see cref="NetworkReady"/> event will fire when the required conditions are met.
        /// That will be the network connection to be up, having a valid IpAddress and optionally for a valid date and time to become available.
        /// </summary>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <exception cref="InvalidOperationException">If any of the <see cref="NetworkHelper"/> methods is called more than once.</exception>
        /// <exception cref="NotSupportedException">There is no network interface configured. Open the 'Edit Network Configuration' in Device Explorer and configure one.</exception>
        public static void SetupNetworkHelper(bool requiresDateTime = false)
        {
            _requiresDateTime = requiresDateTime;

            SetupHelper(true);

            // fire working thread
            new Thread(WorkingThread).Start();
        }

        /// <summary>
        /// This method will setup the network configurations so that the <see cref="NetworkReady"/> event will fire when the required conditions are met.
        /// That will be the network connection to be up, having a valid IpAddress and optionally for a valid date and time to become available.
        /// </summary>
        /// <param name="ipConfiguration">The static IP configuration you want to apply.</param>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <exception cref="NotSupportedException">There is no network interface configured. Open the 'Edit Network Configuration' in Device Explorer and configure one.</exception>
        public static void SetupNetworkHelper(
            IPConfiguration ipConfiguration,
            bool requiresDateTime = false)
        {
            _requiresDateTime = requiresDateTime;
            _ipConfiguration = ipConfiguration;

            SetupHelper(true);

            // fire working thread
            new Thread(WorkingThread).Start();
        }

        /// <summary>
        /// This will wait for the network connection to be up and optionally for a valid date and time to become available.
        /// </summary>
        /// <param name="token">A <see cref="CancellationToken"/> used for timing out the operation.</param>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <returns><see langword="true"/> on success. On failure returns <see langword="false"/> and details with the cause of the failure are made available in the <see cref="Status"/> property</returns>
        public static bool SetupAndConnectNetwork(
            CancellationToken token = default,
            bool requiresDateTime = false)
        {
            return SetupAndConnectNetwork(null, token, requiresDateTime);
        }

        /// <summary>
        /// This will wait for the network connection to be up and optionally for a valid date and time to become available.
        /// </summary>
        /// <param name="ipConfiguration">The static IPv4 configuration to apply to the Ethernet network interface.</param>
        /// <param name="token">A <see cref="CancellationToken"/> used for timing out the operation.</param>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <returns><see langword="true"/> on success. On failure returns <see langword="false"/> and details with the cause of the failure are made available in the <see cref="Status"/> property</returns>
        public static bool SetupAndConnectNetwork(
            IPConfiguration ipConfiguration,
            CancellationToken token = default,
            bool requiresDateTime = false)
        {
            _ipConfiguration = ipConfiguration;

            return InternalWaitNetworkAvailable(
                _workingNetworkInterface,
                ref _networkHelperStatus,
                false,
                token,
                requiresDateTime);
        }

        internal static bool InternalWaitNetworkAvailable(
            NetworkInterfaceType networkInterface,
            ref NetworkHelperStatus helperStatus,
            bool setupEvents,
            CancellationToken token = default,
            bool requiresDateTime = false)
        {
            try
            {
                SetupHelper(setupEvents);

                // loop until cancellation token expires or there is an IP address
                while (!token.IsCancellationRequested && !NetworkHelperInternal.CheckIP(
                    networkInterface,
                    _ipConfiguration))
                {
                    Thread.Sleep(200);
                }

                // handle cancellation token expiration
                if (token.IsCancellationRequested)
                {
                    // update status
                    helperStatus = NetworkHelperStatus.TokenExpiredWaitingIPAddress;

                    // operation failed
                    return false;
                }

                // if DateTime was requested, go for it
                if (requiresDateTime)
                {
                    Debug.WriteLine("Waiting for valid DateTime system clock...");

                    while (!token.IsCancellationRequested && (DateTime.UtcNow.Year < 2021))
                    {
                        Thread.Sleep(200);
                    }
                }

                // handle cancellation token expiration
                if (token.IsCancellationRequested)
                {
                    // update status
                    helperStatus = NetworkHelperStatus.TokenExpiredWaitingDateTime;

                    // operation failed
                    return false;
                }
                else
                {
                    Debug.WriteLine($"We have a valid date {DateTime.UtcNow}");
                }

                // update status
                helperStatus = NetworkHelperStatus.NetworkIsReady;

                // operation succeeded
                return true;
            }
            catch (Exception ex)
            {
                _networkHelperStatus = NetworkHelperStatus.ExceptionOccurred;
                HelperException = ex;

                return false;
            }
        }

        private static void WorkingThread()
        {
            // check if we have an IP
            if(!NetworkHelperInternal.CheckIP(
                _workingNetworkInterface,
                _ipConfiguration))
            {
                // wait here until we have an IP address
                _ipAddressAvailable.WaitOne();
            }

            if (_requiresDateTime)
            {
                // wait until there is a valid DateTime
                NetworkHelperInternal.WaitForValidDateTime();
            }

            // all conditions met
            _networkReady.Set();

            // update flag
            _networkHelperStatus = NetworkHelperStatus.NetworkIsReady;
        }

        private static void AddressChangedCallback(object sender, EventArgs e)
        {
            if(NetworkHelperInternal.CheckIP(
                _workingNetworkInterface,
                _ipConfiguration))
            {
                _ipAddressAvailable.Set();
            }
        }

        /// <summary>
        /// Perform setup of the various fields and events, along with any of the required event handlers.
        /// </summary>
        /// <param name="setupEvents">Set true to setup the events. Required for the thread approach. Not required for the CancelationToken implementation.</param>
        private static void SetupHelper(bool setupEvents)
        {
            if (_helperInstanciated)
            {
                throw new InvalidOperationException();
            }
            else
            {
                // set flag
                _helperInstanciated = true;

                // setup event
                _ipAddressAvailable = new(false);

                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

                if (setupEvents)
                {
                    // check if there are any network interface setup
                    if (nis.Length == 0)
                    {
                        _networkHelperStatus = NetworkHelperStatus.FailedNoNetworkInterface;

                        throw new NotSupportedException();
                    }

                    // setup handler
                    NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);

                    // instantiate events
                    _networkReady = new(false);
                }

                NetworkHelperInternal.InternalSetupHelper(nis, _workingNetworkInterface, _ipConfiguration);

                // update status
                _networkHelperStatus = NetworkHelperStatus.Started;
            }
        }
   
        /// <summary>
        /// Method to reset internal fields to it's defaults
        /// ONLY TO BE USED BY UNIT TESTS
        /// </summary>
        internal static void ResetInstance()
        {
            _ipAddressAvailable = null;
            _networkReady = null;
            _requiresDateTime = false;
            _networkHelperStatus = NetworkHelperStatus.None;
            _helperException = null;
            _ipConfiguration = null;
            _helperInstanciated = false;
        }
    }
}
