using System.Net.Sockets;
using TCPServer.Net.IO;

namespace TCPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter your username: ");
            var username = Console.ReadLine();

            var client = new TcpClient();
            client.Connect("127.0.0.1", 7891);

            var packetReader = new PacketReader(client.GetStream());

            var connectPacket = new PacketBuilder();
            connectPacket.WriteOpCode(0);
            connectPacket.WriteMessage(username);
            client.Client.Send(connectPacket.GetPacketBytes());

            Thread listenThread = new Thread(o => ListenForMessages((TcpClient)o));
            listenThread.Start(client);

            while (true)
            {
                var message = Console.ReadLine();
                var messagePacket = new PacketBuilder();
                messagePacket.WriteOpCode(5);
                messagePacket.WriteMessage(message);
                client.Client.Send(messagePacket.GetPacketBytes());
            }
        }

        static void ListenForMessages(TcpClient client)
        {
            var packetReader = new PacketReader(client.GetStream());
            while (true)
            {
                try
                {
                    var opcode = packetReader.ReadByte();
                    if (opcode == 5)
                    {
                        var message = packetReader.ReadMessage();
                        Console.WriteLine(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Disconnected from server.");
                    break;
                }
            }
        }
    }
}

