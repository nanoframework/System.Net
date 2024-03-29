﻿//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NFUnitTestSocketTests
{
    public class SocketPair
    {
        private const int STANDARD_SIZE = 3;
        public Socket socketClient;
        public Socket socketServer;
        public byte[] bufSend;
        public byte[] bufReceive;
        public IPEndPoint epClient;
        public IPEndPoint epServer;

        public SocketPair(ProtocolType protocolType, SocketType socketType)
        {
            socketClient = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
            socketServer = new Socket(AddressFamily.InterNetwork, socketType, protocolType);

            if (protocolType == ProtocolType.Tcp)
            {
                socketClient.SetSocketOption(SocketOptionLevel.Socket,
                    SocketOptionName.Linger, false);
                socketServer.SetSocketOption(SocketOptionLevel.Socket,
                    SocketOptionName.Linger, false);
            }

            bufSend = null;
            bufReceive = null;
            epClient = null;
            epServer = null;

        }

        ~SocketPair()
        {
            TearDown();
        }

        public void Startup(int portClient, int portServer)
        {
            socketClient.Bind(new IPEndPoint(IPAddress.Loopback, portClient));
            socketServer.Bind(new IPEndPoint(IPAddress.Loopback, portServer));

            epClient = (IPEndPoint)socketClient.LocalEndPoint;
            epServer = (IPEndPoint)socketServer.LocalEndPoint;
            Debug.WriteLine("Server Port#: " + epServer + " Client Port#: " + epClient);

            bufSend = new byte[STANDARD_SIZE];
            new Random().NextBytes(bufSend);

            bufReceive = new byte[bufSend.Length];
        }

        public void TearDown()
        {
            CloseSocket(ref socketClient);
            CloseSocket(ref socketServer);
        }

        private void CloseSocket(ref Socket socket)
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public void AssertDataReceived(int cBytes)
        {
            if (cBytes != bufSend.Length)
                throw new Exception("Recieve failed, wrong size " + cBytes + " " + bufSend.Length);

            for (int i = 0; i < bufReceive.Length; i++)
            {
                if (bufSend[i] != bufReceive[i])
                    throw new Exception("Receive failed, wrong data");
            }
        }
    }
}
