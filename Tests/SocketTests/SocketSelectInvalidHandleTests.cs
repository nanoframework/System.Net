//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System;
using System.Net.Sockets;
using System.Reflection;

namespace NFUnitTestSocketTests
{
    /// <summary>
    /// Makes the native select fail by handing it a socket handle the network stack doesn't know, and checks that
    /// the SocketException reports that failure instead of a stale error left behind by an earlier call.
    /// Only some stacks can fail select this way: the Linux virtual device can, lwIP can't (it drops unknown handles
    /// from the fd sets and reports nothing ready), so the test skips where select can't fail.
    /// </summary>
    [TestClass]
    public class SocketSelectInvalidHandleTests
    {
        // handle that no stack hands out
        private const int UnknownHandle = 0x7FFF0000;

        // errors that report an invalid handle: SOCK_EBADF (WSAEBADF, not in SocketError) and SOCK_ENOTSOCK
        private const int ErrorBadHandle = 10009;

        [TestMethod]
        public void Poll_UnknownHandle_ThrowsInvalidHandleError()
        {
            FieldInfo handleField = typeof(Socket).GetField("m_Handle", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(handleField, "Socket.m_Handle not found through reflection");

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int realHandle = (int)handleField.GetValue(socket);

            SocketException caught = null;
            bool pollResult = false;

            try
            {
                handleField.SetValue(socket, UnknownHandle);

                // zero timeout: select runs once and the call returns without waiting
                pollResult = socket.Poll(0, SelectMode.SelectRead);
            }
            catch (SocketException ex)
            {
                caught = ex;
            }
            finally
            {
                // give the socket its own handle back so Close releases it
                handleField.SetValue(socket, realHandle);
                socket.Close();
            }

            if (caught == null)
            {
                Assert.SkipTest(
                    $"select doesn't fail for an unknown handle on this network stack (Poll returned {pollResult})");
            }

            OutputHelper.WriteLine($"Poll: SocketException ErrorCode={caught.ErrorCode}");

            Assert.IsTrue(
                caught.ErrorCode == ErrorBadHandle || caught.ErrorCode == (int)SocketError.NotSocket,
                $"ErrorCode {caught.ErrorCode} doesn't report an invalid handle (stale error left by an earlier call?)");
        }
    }
}
