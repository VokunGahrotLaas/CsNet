using testNet.Packet.Packets;
using testNet.Packet.PacketType;

namespace testNet {
	class Program {
		public static void Main(string[] args) {
			const int port = 42069;
			Server server = new Server(port);
			Client client = new Client("localhost", port);
			
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