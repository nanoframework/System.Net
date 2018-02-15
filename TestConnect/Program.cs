using System;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Windows.Devices.Gpio;
using System.Reflection;

namespace TestConnect
{
    public class Program
    {
        public static void Main()
        {
            //while (!Debugger.IsAttached) { Thread.Sleep(100); }    // Wait for debugger (only needed for debugging session)
            Thread.Sleep(200);
            Console.WriteLine("Program started");                  // You can remove this line once it outputs correctly on the console
            GpioPin pin = GpioController.GetDefault().OpenPin(27);
            pin.SetDriveMode(GpioPinDriveMode.Output);
            pin.Write(GpioPinValue.High);

            try
            {
                ListenTest(pin);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex.Message);
                // Do whatever please you with the exception caught
            }
            finally    // Enter the infinite loop in all cases
            {
                while (true)
                {
                    pin.Write(GpioPinValue.Low);
                    Thread.Sleep(200);
                    pin.Write(GpioPinValue.High);
                    Thread.Sleep(200);
                }
            }
        }

        static void PinPulse(GpioPin pin, int timeMs)
        {
            pin.Write(GpioPinValue.Low);
            Thread.Sleep(timeMs);
            pin.Write(GpioPinValue.High);
        }

        static void ListenTest(GpioPin pin)
        {
            PinPulse(pin, 2000);

            using (Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4))
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 6501);

                soc.Bind(ep);

                soc.Listen(1);

                while (true)
                {
                    int BytesRead;
                    Socket client = soc.Accept();

                    PinPulse(pin, 500);

                    client.ReceiveTimeout = 10000;

                    try
                    {
                        NetworkStream stream = new NetworkStream(client);

                        byte[] buffer = new byte[100];

                        while ((BytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            PinPulse(pin, 200);
                            stream.Write(buffer, 0, BytesRead);
                        }
                    }
                    catch (Exception) { };

                    client.Close();
                    break;
                }
            }
        }

        //    static void ConnectTest(GpioPin pin)
        //    {

        //        while (true)
        //        {
        //            try
        //            {
        //                using (Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4))
        //                {
        //                    IPAddress ipa = IPAddress.Parse("192.168.2.129");

        //                    IPEndPoint ep = new IPEndPoint(ipa, 6500);

        //                    pin.Write(GpioPinValue.Low);

        //                    Type sType = Type.GetType("System.Net.Sockets.Socket");
        //                    FieldInfo blockingInfo = sType.GetField("m_fBlocking", BindingFlags.NonPublic | BindingFlags.Instance);
        //                    blockingInfo.SetValue(soc, false);

        //                    soc.Connect(ep);

        //                    pin.Write(GpioPinValue.High);
        //                    Console.WriteLine("Connected");

        //                    byte[] buf = new byte[] { 0x41, 0x42, 0x43 };
        //                    soc.Send(buf);
        //                    Console.WriteLine("send");

        //                    soc.ReceiveTimeout = 1000;
        //                    byte[] bufr = new byte[3];
        //                    soc.Receive(bufr);
        //                    Console.WriteLine("receive");

        //                    soc.Close();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error" + ex.Message);
        //            }
        //            finally
        //            {
        //                Thread.Sleep(2000);
        //            }
        //        }
        //        }


    }
}
