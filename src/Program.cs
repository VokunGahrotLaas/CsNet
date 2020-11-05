using System;
using System.Threading.Tasks;
using CsNet.Packet.PacketType;

namespace CsNet {
	public class MyClient: Client {
		public MyClient(string address, int port): base(address, port) { }
		protected override void OnStart() {
			Console.WriteLine($"Client: connected with {Address} on port {Port}");
		}

		protected override void OnClose() {
			Console.WriteLine("Client: closed");
		}

		protected override void OnReceive() {
			PacketString packet = Receive<PacketString>();
			packet.Read(out string message);
			Console.WriteLine($"Client: received '{message}'");
		}
	}
	
	public class MyServer: Server {
		public MyServer(int port): base(port) { }

		protected override void OnStart() {
			Console.WriteLine($"Server: started on port {Port}");
		}

		protected override void OnClose() {
			Console.WriteLine("Server: closed");
		}

		protected override void OnAcceptClient(ServerClient client) {
			Console.WriteLine($"Server: accepted a connection from {client.Address}:{client.Port}");
		}

		protected override void OnRefuseClient(ServerClient client) {
			Console.WriteLine($"Server: refused a connection from {client.Address}:{client.Port}");
		}

		protected override void OnDisconnectClient(ServerClient client) {
			Console.WriteLine($"Server: {client.Address}:{client.Port} got disconnected");
		}

		protected override void OnReceive(ServerClient client) {
			PacketString packet = client.Receive<PacketString>();
			packet.Read(out string message);
			Console.WriteLine($"Server: {client.Address}:{client.Port} sent '{message}'");
		}
	}
	
	public class Program {
		public static void Main() {
			const int port = 42069;
			using MyServer server = new MyServer(port);
			using MyClient client1 = new MyClient("localhost", port);
			using MyClient client2 = new MyClient("localhost", port);
			
			server.Start();
			client1.Start();
			client2.Start();

			client1.Send(new PacketString("hey :)"));
			client2.Send(new PacketString("wsh :)"));
			
			server.SendAll(new PacketString("Hi !"));
			
			client1.Close();
			server.SendAll(new PacketString("Hi again !"));
		}
	}
}