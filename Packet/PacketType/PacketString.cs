using System;
using System.Net;
using System.Text;

namespace testNet.Packet.PacketType {
	public class PacketString: PacketType<string> {
		public override void Read(out string data) {
			data = Encoding.UTF8.GetString(Read().Span);
		}

		public override void Write(string data) {
			Write(Encoding.UTF8.GetBytes(data));
		}
	}
}