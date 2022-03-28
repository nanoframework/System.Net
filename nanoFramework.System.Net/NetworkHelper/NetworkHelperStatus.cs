using System;

namespace nanoFramework.Networking
{
    /// <summary>
    /// Error conditions of <see cref="NetworkHelper"/>.
    /// </summary>
    public enum NetworkHelperStatus
    {
        /// <summary>
        /// No error specified
        /// </summary>
        None,

        /// <summary>
        /// NetworkHelper was started and it's waiting for the requested network condition to be met.
        /// </summary>
        Started,

        /// <summary>
        /// All requested conditions where met and the network is available.
        /// </summary>
        NetworkIsReady,

        /// <summary>
        /// Failed to execute because there is no network interface configured.
        /// </summary>
        FailedNoNetworkInterface,

        /// <summary>
        /// Token expired while waiting for a valid IP address.
        /// </summary>
        TokenExpiredWaitingIPAddress,

        /// <summary>
        /// Token expired while waiting for a valid <see cref="DateTime"/>.
        /// </summary>
        TokenExpiredWaitingDateTime,

        /// <summary>
        /// There was an error connecting to WiFi when performing the scan.
        /// </summary>
        ErrorConnetingToWiFiWhileScanning,

        /// <summary>
        /// An exception occurred with waiting for the network to become ready. Check HelperException property to find the <see cref="Exception"/> that was thrown.
        /// </summary>
        ExceptionOccurred
    }
}
