using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace MyNewService
{

    public class SynchronousSocketClient
    {

        public static void StartClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine(String.Format("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString()));


                    while (true)
                    {

                        String msg = ReciveMessageFromServer(sender);

                        HandleCommand(msg, sender);


                        if (msg.Equals("done "))
                        {
                            break;
                        }

                    }

                    // Release the socket.  
                    Console.WriteLine("closing connection");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}" + ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}" + se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}" + e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static void Wait(TimeSpan t)
        {
            Thread.Sleep(t);
            Console.WriteLine("i finished waiting: " + t);

        }

        public static void HandleCommand(String command, Socket sender)
        {
            Console.WriteLine(command.Substring(0, command.IndexOf(" ")));
            switch (command.Substring(0, command.IndexOf(" ")))
            {
                case "time":
                    DateTime timeNow = DateTime.Now;
                    DateTime inputTime = new DateTime(2020, 12, 27, 9, 12, 0);
                    TimeSpan value1 = inputTime.Subtract(timeNow);
                    Console.WriteLine("waiting: " + value1);
                    Thread thread = new Thread(() => Wait(value1));
                    thread.Start();
                    break;
                case "done":
                    Console.WriteLine("the server has requested to disconnect");
                    break;
                case "chrome":
                    Console.WriteLine("opening chrome");
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe");
                    break;
                case "ipconfig":
                    Console.WriteLine("runing ipconfig");
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = @"/C ipconfig > C:\Users\david\Desktop\test\ipconfig.txt";
                    process.StartInfo = startInfo;
                    process.Start();
                    break;
                case "file":
                    // There is a text file test.txt located in the root directory.
                    string fileName = "C:\\Users\\david\\Desktop\\test\\test1.txt";
                    // Send file fileName to remote device
                    Console.WriteLine("Sending {0} to the host." + fileName);
                    // Send the data through the socket.  
                    string readText = File.ReadAllText(fileName) + "<EOF>";
                    byte[] msg = Encoding.ASCII.GetBytes(readText);
                    sender.Send(msg);
                    break;
                default:
                    Console.WriteLine("command not recognized");
                    break;


            }
        }

        public static String ReciveMessageFromServer(Socket sender)
        {
            byte[] size = new byte[10];

            // Receive the response from the remote device.  
            int sizeLen = sender.Receive(size);


            byte[] bytes = new byte[sizeLen];
            int bytesRec = sender.Receive(bytes);

            String msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return msg;
        }



        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }
    }
}