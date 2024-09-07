using System.Net;
using System.Net.Sockets;
using TCPServer.Net.IO;

namespace TCPServer
{
    internal class Program
    {
        private static List<Client> _clients = new List<Client>();
        private static TcpListener _listener;

        private static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var newClient = new Client(client);
                _clients.Add(newClient);

                BroadcastConnection();
            }
        }

        private static void BroadcastConnection()
        {
            foreach (var client in _clients)
            {
                foreach (var c in _clients)
                {
                    var packet = new PacketBuilder();
                    packet.WriteOpCode(1);
                    packet.WriteMessage(c.Username);
                    client.ClientSocket.Client.Send(packet.GetPacketBytes());
                }
            }
        }

        public static void BroadcastMessage(string message)
        {
            foreach (var client in _clients)
            {
                var packet = new PacketBuilder();
                packet.WriteOpCode(5);
                packet.WriteMessage(message);
                client.ClientSocket.Client.Send(packet.GetPacketBytes());
            }
        }
    }

    public class Client
    {
        public TcpClient ClientSocket { get; set; }
        public string Username { get; set; }
        private PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            _packetReader = new PacketReader(client.GetStream());

            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{Username}] has connected.");

            Task.Run(() => Process());
        }

        private void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    if (opcode == 5)
                    {
                        var message = _packetReader.ReadMessage();
                        Console.WriteLine($"{Username}: {message}");
                        Program.BroadcastMessage($"{Username}: {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{Username}] disconnected.");
                    break;
                }
            }
        }
    }
}