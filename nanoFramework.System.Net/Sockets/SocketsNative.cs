using System;
using System.Runtime.CompilerServices;


namespace System.Net.Sockets
{
   
    internal class NativeSocket
    {
        public const int FIONREAD = 0x4004667F;

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int socket(int family, int type, int protocol);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void bind(object socket, EndPoint address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void connect(object socket, EndPoint endPoint, bool fThrowOnWouldBlock);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int send(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int recv(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int close(object socket);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void listen(object socket, int backlog);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int accept(object socket);

        //No standard non-blocking api
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void getaddrinfo(string name, out string canonicalName, out byte[][] addresses);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void shutdown(object socket, int how, out int err);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int sendto(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms, EndPoint endPoint);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int recvfrom(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms, ref EndPoint endPoint);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void getpeername(object socket, out EndPoint endPoint);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void getsockname(object socket, out EndPoint endPoint);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void getsockopt(object socket, int level, int optname, byte[] optval);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void setsockopt(object socket, int level, int optname, byte[] optval);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool poll(object socket, int mode, int microSeconds);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ioctl(object socket, uint cmd, ref uint arg);
    }
}





