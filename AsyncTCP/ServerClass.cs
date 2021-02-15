using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AsyncTCP
{
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    internal class ServerClass
    {
        private static readonly Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static readonly List<Socket> ClientSocketsList = new List<Socket>();
        public const int BufferSize = 2048; //2mb buffer
        private static int _backlog = 10;
        private static readonly byte[] Buffer = new byte[BufferSize];

        public static void Start(IPAddress ipAddress, int port)
        {
            ServerSocket.Bind(new IPEndPoint(ipAddress, port));
            ServerSocket.Listen(_backlog);
            //Console.WriteLine("[DEBUG] Setting up server...");
            ServerSocket.BeginAccept(AcceptCallback, null);//start accepting clients
            //Console.WriteLine("[DEBUG] Server setup complete");
        }

        public static void CloseServer()
        {
            foreach (Socket socket in ClientSocketsList)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            ServerSocket.Close();
        }

        public static void SendData(Socket socket, byte[] dataBytes)
        {
            
            socket.Send(dataBytes);//encrypt data bytes (todo)
            socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, socket);
        }

        private static void AcceptCallback(IAsyncResult asyncResult)
        {
            Socket socket = ServerSocket.EndAccept(asyncResult);
            ClientSocketsList.Add(socket);//add socket to List
            //Console.WriteLine("[DEBUG] New Client Added");
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveCallback, socket);//write data to buffer
            ServerSocket.BeginAccept(AcceptCallback, null);//begin accepting new clients again
        }

        private static bool IsSocketConnected(Socket s)
        {
            try
            {
                return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
            }
            catch
            {
                return false;
            }

            /* The long, but simpler-to-understand version:

                    bool part1 = s.Poll(1000, SelectMode.SelectRead);
                    bool part2 = (s.Available == 0);
                    if ((part1 && part2 ) || !s.Connected)
                        return false;
                    else
                        return true;

            */
        }

        private static void ReceiveCallback(IAsyncResult asyncResult)//receive data
        {
            Socket currentSocket = (Socket)asyncResult.AsyncState;

            int receiveSize;

            try
            {
                receiveSize = currentSocket.EndReceive(asyncResult);
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("Cannot access a disposed object."))
                {
                    return;
                }
                else
                {
                    Console.WriteLine(e);
                    return;
                }
            }

            byte[] recBuf = new byte[receiveSize];
            Array.Copy(Buffer, recBuf, receiveSize);
            //check first few bytes to know data type

            ////////////////////
            ///TEST TCP STUFF///
            ////////////////////
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);
            byte[] testdata = Encoding.ASCII.GetBytes(text);//just echoes stuff
            SendData(currentSocket, testdata);
            if (text.Contains("exit"))
            {
                currentSocket.Shutdown(SocketShutdown.Both);
                currentSocket.Close();
                ClientSocketsList.Remove(currentSocket);
                //Console.WriteLine("Client Requested Disconnect");
                return;
            }
            Console.WriteLine(IsSocketConnected(currentSocket));
            if (IsSocketConnected(currentSocket))
            {
                currentSocket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, currentSocket);
            }
            else
            {
                try
                {
                    currentSocket.Shutdown(SocketShutdown.Both);
                    currentSocket.Close();
                    ClientSocketsList.Remove(currentSocket);
                    //Console.WriteLine("Client disconnected ungracefully");
                }
                catch
                {
                    try { ClientSocketsList.Remove(currentSocket); }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }
}