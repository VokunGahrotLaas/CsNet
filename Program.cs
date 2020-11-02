using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using testNet.Packet.Packets;
using testNet.Packet.PacketType;

namespace testNet {
	public class MyClient: Client {
		public MyClient(string address, int port): base(address, port) { }
		protected override void OnStart() {
			Console.WriteLine($"Client: connected with {Address} on port {Port} !");
		}

		protected override void OnClose() {
			Console.WriteLine($"Client: closed !");
		}

		protected override void OnReceive() {
			Packets packet = Receive<Packets>();
			
			PacketString packetString2 = packet.Get<PacketString>();
			packetString2.Read(out string message2);
        		
			PacketString packetString1= packet.Get<PacketString>();
			packetString1.Read(out string message1);
        		
			Console.WriteLine($"Client: received '{message1}' & '{message2}'");
		}
	}
	
	public class MyServer: Server {
		public MyServer(int port): base(port) { }

		protected override void OnStart() {
			Console.WriteLine($"Server: started on port {Port} !");
		}

		protected override void OnClose() {
			Console.WriteLine($"Server: closed !");
		}

		protected override void OnAddClient(ServerClient client) {
			Console.WriteLine($"Server: registered a connection from {client.Address} on port {client.Port}.");
		}

		protected override void OnRemoveClient(ServerClient client) {
			Console.WriteLine($"Server: {client.Address} got disconnected.");
		}

		protected override void OnReceive(ServerClient client) {
			Packets packet = client.Receive<Packets>();
			
			PacketString packetString2 = packet.Get<PacketString>();
			packetString2.Read(out string message2);
        		
			PacketString packetString1= packet.Get<PacketString>();
			packetString1.Read(out string message1);
        		
			Console.WriteLine($"Server: received '{message1}' & '{message2}'");
		}
	}
	
	class Program {
		static void Main(string[] args) {
			const int port = 42069;
			MyServer server = new MyServer(port);
			MyClient client = new MyClient("localhost", port);
			
			server.Start();
			client.Start();

			PacketString stringPacket1 = new PacketString();
			stringPacket1.Write("machin");

			PacketString stringPacket2 = new PacketString();
			stringPacket2.Write("truc");
			
			Packets packet = new Packets();
			packet.Add(stringPacket1);
			packet.Add(stringPacket2);
			client.Send(packet);

			server.Close();
			client.Close();
		}
	}
}