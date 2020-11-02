using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using testNet.Packet;

namespace testNet {
	public class Client: IDisposable {
		private bool _disposed = false;
		private readonly int _port;
		private readonly TcpClient _tcpClient;
		private readonly NetworkStream _stream;
		private Task _task;
		private bool _isOpen = false;
		
		public string Address { get; }

		public int Port { get; }
		public NetworkStream Stream { get; }
		public bool IsOpen { get; }
		public bool DataAvailable => _stream.DataAvailable;

		public Client(string address, int port) {
			Address = address;
			_port = port;
			_tcpClient = new TcpClient(address, port);
			_stream = _tcpClient.GetStream();
		}
		
		public Client(TcpClient tcpClient) {
			Address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
			_port = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
			_tcpClient = tcpClient;
			_stream = _tcpClient.GetStream();
		}

		~Client() => Dispose(false);
		
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			if (_isOpen)
				Close();
			if (disposing) {
				_tcpClient.Dispose();
				_stream.Dispose();
			}
			_disposed = true;
		}

		public void Start() {
			if (_isOpen) return;
			_isOpen = true;
			OnStart();
			_task = Run();
		}

		public void Close() {
			if (!_isOpen) return;
			_isOpen = false;
			_task.Wait();
			OnClose();
			_tcpClient.Close();
		}

		public void CloseForce() {
			if (!_isOpen) return;
			_isOpen = false;
			_task.Dispose();
			_tcpClient.Close();
		}

		protected virtual void OnStart() {
			//
		}
		
		protected virtual async Task Run() {
			//
		}

		protected virtual void OnClose() {
			//
		}

		public void Send(APacket packet) {
			packet.Send(_stream);
		}

		public async Task SendAsync(APacket packet) {
			await packet.SendAsync(_stream);
		}

		public SubPacket Receive<SubPacket>() where SubPacket: APacket, new() {
			SubPacket packet = new SubPacket();
			packet.Receive(_stream);
			return packet;
		}

		public async Task<SubPacket> ReceiveAsync<SubPacket>() where SubPacket: APacket, new() {
			SubPacket packet = new SubPacket();
			await packet.ReceiveAsync(_stream);
			return packet;
		}
	}
}