using System;

namespace testNet.Packet.Packets {
	public abstract class APackets: APacket {
		private readonly byte _size;
		protected APackets(byte size) { _size = size; }
		
		public virtual void Add(APacket packet) {
			Memory<byte> packetMem = new Memory<byte>(new byte[packet.Length + _size]);
			int length = packet.Length;
			for (int i = 0; i < _size; ++i) {
				packetMem.Span[packet.Length + _size - i - 1] = (byte)(length & 255);
				length >>= 8;
			}
			if (length != 0) throw new Exception($"{nameof(Add)}: packet was too big to add!");
			packet.Read().CopyTo(packetMem);
			Write(packetMem);
			
			/*Console.Write("Packets Added: '");
			foreach (byte b in packetMem.Span)
				Console.Write($"{b:X2}");
			Console.Write("' | State: '");
			foreach (byte b in _bytes.Span)
				Console.Write($"{b:X2}");
			Console.WriteLine("'");*/
		}
		
		public virtual SubPacket Get<SubPacket>() where SubPacket: APacket, new() {
			/*Console.Write("Packets Got: '");
			foreach (byte b in _bytes.Span)
				Console.Write($"{b:X2}");
			Console.WriteLine("'");*/
			
			int length = 0;
			foreach (byte b in Read(_size).Span) {
				length <<= 8;
				length |= (int)b;
			}
			SubPacket packet = new SubPacket();
			packet.Write(Read(length));
			return packet;
		}
	}
	
	public class ShortPackets: APackets {
		protected byte _size;
		public ShortPackets(): base(2) {  }
	}
	public class Packets: APackets {
		protected byte _size;
		public Packets(): base(4) {  }
	}
	public class LongPackets: APackets {
		protected byte _size;
		public LongPackets(): base(8) {  }
	}
}