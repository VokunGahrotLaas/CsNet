using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace testNet.Packet {
	public abstract class APacket {
		protected Memory<byte> _bytes = Memory<byte>.Empty;
		public int Length => _bytes.Length;

		public void Write(Memory<byte> bytes) {
			byte[] array = _bytes.ToArray();
			Array.Resize(ref array, _bytes.Length + bytes.Length);
			bytes.ToArray().CopyTo(array, _bytes.Length);
			_bytes = new Memory<byte>(array);
			
			/*Console.Write("Packet Written: '");
			foreach (byte b in _bytes.Span)
				Console.Write($"{b:X2}");
			Console.WriteLine("'");*/
		}
		
		public Memory<byte> Read() {
			Memory<byte> ret = _bytes;
			_bytes = Memory<byte>.Empty;
			return ret;
		}
		
		protected Memory<byte> Read(int length) {
			if (length > _bytes.Length) throw new ArgumentOutOfRangeException($"{nameof(length)}", $"{length}", $"public byte[] testNet.Packet.Read(int): length cannot be superior to packet size {_bytes.Length}");
			Memory<byte> ret = _bytes.Slice(_bytes.Length - length, length);
			_bytes = _bytes.Slice(0, _bytes.Length - length);
			return ret;
		}
		
		public void Send<T>(T stream) where T: Stream {
			stream.Write(_bytes.Span);
		}
		
		public async Task SendAsync<T>(T stream) where T: Stream {
			await stream.WriteAsync(_bytes);
		}

		public void Receive(NetworkStream stream) {
			byte[] bytes = Array.Empty<byte>();
			Array.Resize(ref bytes, 1024);
			while (stream.DataAvailable) {
				int bytesRead = stream.Read(bytes, 0, bytes.Length);
				Write(new Memory<byte>(bytes).Slice(0, bytesRead));
			}
		}

		public async Task ReceiveAsync(NetworkStream stream) {
			byte[] bytes = new byte[1024];
			while (stream.DataAvailable) {
				int bytesRead = await stream.ReadAsync(bytes, 0, bytes.Length);
				Write(new Memory<byte>(bytes).Slice(0, bytesRead));
			}
		}
	}
}