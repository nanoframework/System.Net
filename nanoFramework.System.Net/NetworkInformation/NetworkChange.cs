//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Contains argument values for network availability events.
    /// </summary>
    public class NetworkAvailabilityEventArgs : EventArgs
    {
#pragma warning disable IDE0032 // nanoFramework doesn't support auto-properties
        private readonly bool _isAvailable;
#pragma warning disable IDE0032 // nanoFramework doesn't support auto-properties

        internal NetworkAvailabilityEventArgs(bool isAvailable)
        {
            _isAvailable = isAvailable;
        }

        /// <summary>
        /// Indicates whether the network is currently available.
        /// </summary>
        public bool IsAvailable => _isAvailable;
    }

    /// <summary>
    /// Contains argument values for network availability events.
    /// </summary>
    public class NetworkAPStationEventArgs : EventArgs
    {
        private readonly int _stationIndex;
        private readonly bool _isConnected;

        internal NetworkAPStationEventArgs(bool isConnected, int StationIndex)
        {
            _isConnected = isConnected;
            _stationIndex = StationIndex;
        }

        /// <summary>
        /// Indicates whether the client has connected or disconnected.
        /// </summary>
        public bool IsConnected { get => _isConnected;  }

        /// <summary>
        /// Returns the Index of the connected Station.
        /// </summary>
        public int StationIndex { get => _stationIndex; }
    }


    /// <summary>
    /// Provides an event handler that is called when the network address changes.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the network address changed event. </param>
    /// <param name="e">Contains the network address changed event arguments. </param>
    public delegate void NetworkAddressChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Indicates a change in the availability of the network.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the network availability changed event. </param>
    /// <param name="e">Contains the network availability changed event arguments. </param>
    public delegate void NetworkAvailabilityChangedEventHandler(object sender, NetworkAvailabilityEventArgs e);

    /// <summary>
    /// Indicates a change in the connected clients to Access Point.
    /// </summary>
    /// <param name="NetworkIndex">Specifies the index of network interface that sent the event. </param>
    /// <param name="e">Contains the network AP client changed event arguments. </param>
    public delegate void NetworkAPStationChangedEventHandler(int NetworkIndex, NetworkAPStationEventArgs e);

    /// <summary>
    /// Contains information about changes in the availability and address of the network.
    /// </summary>
    public static class NetworkChange
    {
        [Flags]
        internal enum NetworkEventType : byte
        {
            Invalid = 0,
            AvailabilityChanged = 1,
            AddressChanged = 2,
            APStationChanged = 3,
        }

        [Flags]
        internal enum NetworkEvents : byte
        {
            NetworkAvailable = 0x1,
        }

        internal class NetworkEvent : BaseEvent
        {
            public NetworkEventType EventType;
            public byte Flags;
            public ushort Index;
            public ushort Data;
            public DateTime Time;
        }

        internal class NetworkChangeListener : IEventListener, IEventProcessor
        {
            public void InitializeForEventSource()
            {
                // Method intentionally left empty.
            }

            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                NetworkEvent networkEvent = new NetworkEvent();
                networkEvent.EventType = (NetworkEventType)(data1 & 0xFF);
                networkEvent.Flags = (byte)((data1 >> 16) & 0xFF);

                // Data2 - Low 8 bits are the Network interface index
                //         Top 8 bits extra data ( i.e AP station index )
                networkEvent.Index = (ushort)(data2 & 0xff);
                networkEvent.Data =  (ushort)(data2 >> 8);
                networkEvent.Time = time;

                return networkEvent;
            }

            public bool OnEvent(BaseEvent ev)
            {
                if (ev is NetworkEvent myEvent)
                {
                    OnNetworkChangeCallback(myEvent);
                }

                return true;
            }
        }

        /// <summary>
        /// Event occurs when the IP address of a network interface changes.
        /// </summary>
        /// <remarks>
        /// The NetworkChange class raises NetworkAddressChanged events when the address of a network interface, 
        /// also called a network card or adapter, changes.
        /// 
        /// To have a NetworkChange object call an event-handling method when a NetworkAddressChanged event occurs, 
        /// you must associate the method with a NetworkAddressChangedEventHandler delegate, and add this delegate to this event. 
        /// </remarks>
        public static event NetworkAddressChangedEventHandler NetworkAddressChanged;

        /// <summary>
        /// Event occurs when the availability of the network changes.
        /// </summary>
        /// <remarks>
        /// The NetworkChange class raises NetworkAvailabilityChanged events when the availability of the network changes. 
        /// The network is available when at least one network interface is marked "up" and is not a tunnel or loopback interface.
        /// 
        /// To have a NetworkChange object call an event-handling method when a NetworkAvailabilityChanged event occurs, 
        /// you must associate the method with a NetworkAvailabilityChangedEventHandler delegate, and add this delegate to this event. 
        /// </remarks>
        public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged;

        /// <summary>
        /// Event occurs when a station connects or disconnects from Soft Access Point.
        /// </summary>
        /// <remarks>
        /// The NetworkChange class raises the NetworkAPStationChanged events when a client 
        /// connects or disconnects from the Soft AP. 
        /// 
        /// To have a NetworkChange object call an event-handling method when a NetworkAPStationChanged event occurs, 
        /// you must associate the method with a NetworkAPStationChangedEventHandler delegate, and add this delegate to this event. 
        /// </remarks>
        public static event NetworkAPStationChangedEventHandler NetworkAPStationChanged;

        static NetworkChange()
        {
            NetworkChangeListener networkChangeListener = new NetworkChangeListener();

            EventSink.AddEventProcessor(EventCategory.Network, networkChangeListener);
            EventSink.AddEventListener(EventCategory.Network, networkChangeListener);
        }


        internal static void OnNetworkChangeCallback(NetworkEvent networkEvent)
        {
     
            switch (networkEvent.EventType)
            {
                case NetworkEventType.AvailabilityChanged:
                    if (NetworkAvailabilityChanged != null)
                    {
                        bool isAvailable = ((networkEvent.Flags & (byte)NetworkEvents.NetworkAvailable) != 0);
                        NetworkAvailabilityEventArgs args = new NetworkAvailabilityEventArgs(isAvailable);

                        NetworkAvailabilityChanged(networkEvent.Index, args);
                    }
                    break;

                case NetworkEventType.AddressChanged:
                    if (NetworkAddressChanged != null)
                    {
                        EventArgs args = new EventArgs();
                        NetworkAddressChanged(networkEvent.Index, args);
                    }

                    break;

                case NetworkEventType.APStationChanged:
                    if (NetworkAPStationChanged != null)
                    {
                        bool isConnected = ((networkEvent.Flags & (byte)NetworkEvents.NetworkAvailable) != 0);
                            
                        NetworkAPStationEventArgs args = new NetworkAPStationEventArgs(isConnected, networkEvent.Data);
                    
                        NetworkAPStationChanged(networkEvent.Index, args);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
