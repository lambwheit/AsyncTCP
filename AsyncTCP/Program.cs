using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Threading;

namespace AsyncTCP
{
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    internal class Program
    {
        private static void Main()
        {
            Console.Title = "Async TCP Server";
            ServerClass.Start(IPAddress.Any, 100);
            new Thread(UpdateListData).Start();
            while (true)
            {
                Console.WriteLine("Select Client");
                int client = int.Parse(Console.ReadLine() ?? string.Empty);
                Console.WriteLine("What do you want to send");
                byte[] tosend = Encoding.ASCII.GetBytes(Console.ReadLine() ?? string.Empty);
                ServerClass.SendData(ServerClass.ClientSocketsList[client], tosend);
                Console.WriteLine("Do you want to shut down the server? ");
                string option = Console.ReadLine();
                if (option == "killme")
                {
                    ServerClass.CloseServer();
                    Console.WriteLine("Server Closed");
                    break;
                }
                else
                {
                }
            }
            Console.WriteLine("Press Enter To Exit");
            Console.ReadLine();
        }

        private static void UpdateListData()
        {
            int count = 0;
            while (true)
            {
                if (ServerClass.ClientSocketsList.Count != count)
                {
                    if (ServerClass.ClientSocketsList.Count < count)
                    {
                        Console.WriteLine("Client Removed");
                    }
                    else
                    {
                        Console.WriteLine("Client Added");
                    }
                    count = ServerClass.ClientSocketsList.Count;
                    foreach (var variable in ServerClass.ClientSocketsList)
                    {
                        Console.WriteLine(((IPEndPoint)(variable.RemoteEndPoint)).Address.ToString());
                    }
                }
            }
        }
    }
}