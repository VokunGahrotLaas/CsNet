using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using testNet.Packet.Packets;
using testNet.Packet.PacketType;


namespace testNet {
	public class Server: IDisposable {
		private bool _disposed = false;
		private readonly int _port;
		private readonly TcpListener _tcpListener;
		private Task _task;
		private bool _isOpen = false;

		public int Port { get;  }
		public bool IsOpen { get; }

		public Server(int port) {
			_port = port;
			_tcpListener = new TcpListener(IPAddress.Any, port);
			_tcpListener.Start();
		}

		~Server() => Dispose(false);
		
		public void Dispose() {
			Dispose(true);
		}
		
		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			if (_isOpen)
				Close();
			if (disposing) { }
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
			_tcpListener.Stop();
		}

		public void CloseForce() {
			if (!_isOpen) return;
			_isOpen = false;
			_task.Dispose();
			_tcpListener.Stop();
		}

		protected virtual void OnStart() {
			//
		}
		
		protected virtual async Task Run() {
			while (_isOpen) {
				Client client = new Client(await _tcpListener.AcceptTcpClientAsync());
				client.Start();
				Console.WriteLine($"Server Accepted Connection: {client.Address} {client.Port}");
				while (!client.DataAvailable) { await Task.Delay(1); }
				Packets packet = client.Receive<Packets>();
				
				PacketString packetString2 = packet.Get<PacketString>();
				packetString2.Read(out string message2);
				
				PacketString packetString1= packet.Get<PacketString>();
				packetString1.Read(out string message1);
				
				Console.WriteLine($"Server Received: '{message1}' & '{message2}'");
			}
		}

		protected virtual void OnClose() {
			//
		}
	}
}