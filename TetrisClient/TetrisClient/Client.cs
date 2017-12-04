using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// Devs: Emrin ANGELOV
// Devs: Xavier BETIS

/// <summary>
/// CLIENT : displays table received by server and interacts with user.
/// To end game, server sends the message "Game Over" and disconnects.
/// Outputs: keywords "high", "left", and "right" to the server.
/// </summary>

public class SynchronousSocketClient
{
    public static void StartClient(int refreshRate, IPAddress ipAddress,
        int server_port, string high_key, string right_key, string low_key, string left_key)
    {
        bool playing = true;  
        byte[] bytes = new byte[1024]; // Data buffer for incoming data.

        try // Connect to a remote device. 
        {
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, server_port); //Remote end point with ip and port
            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // Create a TCP/IP  socket.  

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);
                Console.WriteLine("Connected to {0}", sender.RemoteEndPoint.ToString());
                
                byte[] msg;
                int bytesSent;
                int bytesRec;
                string server_reply;
                string pressed_key;

                Console.WriteLine("--------------------------");
                Console.WriteLine("Waiting for other players...");
                Console.WriteLine("--------------------------");
                ConsoleKeyInfo cki;
                
                do // while playing
                {
                    while (playing && !Console.KeyAvailable) //while no key is pressed, just listen to server
                    {
                        bytesRec = sender.Receive(bytes); // Receive the response from the remote device.
                        server_reply = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        Console.Clear();
                        Console.WriteLine(server_reply); // Display response
                        if (server_reply.Equals("Game Over")) { playing = false; };
                        Thread.Sleep(refreshRate);
                    }
                    if(!playing) { break; }
                    // exiting the while loop means a key was pressed, so we send it to the server if it's valid
                    cki = Console.ReadKey(true);
                    pressed_key = cki.Key.ToString();

                    if (pressed_key == high_key) // only send to server if pressed key is configured
                    {
                        msg = Encoding.ASCII.GetBytes("high<EOF>");
                        bytesSent = sender.Send(msg); // Send the data through the socket.
                    }
                    if (pressed_key == right_key)
                    {
                        msg = Encoding.ASCII.GetBytes("right<EOF>");
                        bytesSent = sender.Send(msg);
                    }
                    if (pressed_key == low_key)
                    {
                        msg = Encoding.ASCII.GetBytes("low<EOF>");
                        bytesSent = sender.Send(msg);
                    }
                    if (pressed_key == left_key)
                    {
                        msg = Encoding.ASCII.GetBytes("left<EOF>");
                        bytesSent = sender.Send(msg);
                    }
                } while (playing);

                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

                Console.WriteLine("You lost hahahha!");
            }
            /////////////// EXCEPTIONS ///////////////////
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        // Default Settings
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // Establish the remote endpoint for the socket.
        string server_address = ipHostInfo.AddressList[0].ToString(); // cmd => ipconfig => Link-local IPv6 Address
        int server_port = 11000;
        string high_key = "W";
        string right_key = "D";
        string low_key = "S";
        string left_key = "A";

        if (args.Length == 6) // Customized Settings
        {
            try
            {
                server_address = args[0];
                server_port = Int32.Parse(args[1]);
                high_key = args[2]; // THESE MUST BE CAPITAL LETTERS
                right_key = args[3];
                low_key = args[4];
                left_key = args[5];
                Console.WriteLine("Custom settings have been applied.");
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        IPAddress ipAddress = IPAddress.Parse(server_address);
        int refreshRate = 50; //ms

        StartClient(refreshRate, ipAddress, server_port, high_key, right_key, low_key, left_key);

        Console.WriteLine("Finishged application! Press any key to close.");
        Console.ReadKey();
        return 0;
    }
}