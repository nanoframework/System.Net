////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using nanoFramework.Runtime.Events;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Contains argument values for network availability events.
    /// </summary>
    public class NetworkAvailabilityEventArgs : EventArgs
    {
        private bool _isAvailable;

        internal NetworkAvailabilityEventArgs(bool isAvailable)
        {
            _isAvailable = isAvailable;
        }

        /// <summary>
        /// Indicates whether the network is currently available.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return _isAvailable;
            }
        }
    }

    /// <summary>
    /// Provides an event handler that is called when the network address changes.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the network address changed event. </param>
    /// <param name="e">Contains the network address changed event arguments. </param>
    public delegate void NetworkAvailabilityChangedEventHandler(Object sender, NetworkAvailabilityEventArgs e);

    /// <summary>
    /// Indicates a change in the availability of the network.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the network availability changed event. </param>
    /// <param name="e">Contains the network availability changed event arguments. </param>
    public delegate void NetworkAddressChangedEventHandler(Object sender, EventArgs e);

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
        }

        [Flags]
        internal enum NetworkEventFlags : byte
        {
            NetworkAvailable = 0x1,
        }

        internal class NetworkEvent : BaseEvent
        {
            public NetworkEventType EventType;
            public byte Flags;
            public DateTime Time;
        }

        internal class NetworkChangeListener : IEventListener, IEventProcessor
        {
            public void InitializeForEventSource()
            {
            }

            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                NetworkEvent networkEvent = new NetworkEvent();
                networkEvent.EventType = (NetworkEventType)(data1 & 0xFF);
                networkEvent.Flags = (byte)((data1 >> 16) & 0xFF);
                networkEvent.Time = time;

                return networkEvent;
            }

            public bool OnEvent(BaseEvent ev)
            {
                if (ev is NetworkEvent)
                {
                    NetworkChange.OnNetworkChangeCallback((NetworkEvent)ev);
                }

                return true;
            }
        }

        /// Events
        public static event NetworkAddressChangedEventHandler NetworkAddressChanged;
        public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged;

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
                    {
                        if (NetworkAvailabilityChanged != null)
                        {
                            bool isAvailable = ((networkEvent.Flags & (byte)NetworkEventFlags.NetworkAvailable) != 0);
                            NetworkAvailabilityEventArgs args = new NetworkAvailabilityEventArgs(isAvailable);

                            NetworkAvailabilityChanged(null, args);
                        }
                        break;
                    }
                case NetworkEventType.AddressChanged:
                    {
                        if (NetworkAddressChanged != null)
                        {
                            EventArgs args = new EventArgs();
                            NetworkAddressChanged(null, args);
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}


