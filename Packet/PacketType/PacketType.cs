using System;

namespace testNet.Packet.PacketType {
	public abstract class PacketType<T>: APacket {
		public PacketType() { }
		public PacketType(T str) { Write(str); }
		public abstract void Read(out T data);
		public abstract void Write(T data);
	}
}