using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using CsNet.Packet;

namespace CsNet {
	public class Client: IDisposable {
		private bool _disposed = false;
		private readonly TcpClient _tcpClient;
		private Task _run_task;
		private Task _verify_task;

		public string Address { get; }
		public int Port { get; }
		public NetworkStream Stream { get; }
		public bool IsOpen { get; private set; } = false;
		public bool IsDataAvailable => Stream.DataAvailable;
		public int DataAvailable => _tcpClient.Available;
		public bool Connected => _tcpClient.Connected;

		public Client(string address, int port) {
			Address = address;
			Port = port;
			_tcpClient = new TcpClient(address, port);
			Stream = _tcpClient.GetStream();
		}

		public Client(TcpClient tcpClient) {
			Address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
			Port = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
			_tcpClient = tcpClient;
			Stream = _tcpClient.GetStream();
		}

		~Client() => Dispose(false);
		
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			if (IsOpen)
				Close();
			if (disposing) {
				_run_task.Dispose();
				_verify_task.Dispose();
				_tcpClient.Dispose();
				Stream.Dispose();
			}
			_disposed = true;
		}

		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			OnStart();
			_run_task = Run();
			_verify_task = Verify();
		}

		public void Close() {
			if (!IsOpen) return;
			IsOpen = false;
			_run_task.Wait();
			_verify_task.Wait();
			OnClose();
			_tcpClient.Close();
		}

		private async Task Verify() {
			while (IsOpen) {
				if (!Connected)
					Disconnect();
				else if (IsDataAvailable)
					OnReceive();
				await Task.Delay(1);
			}
		}

		private void Disconnect() {
			OnDisconnect();
			Close();
		}

		public void Send(APacket packet) {
			packet.Send(Stream);
		}

		public async Task SendAsync(APacket packet) {
			await packet.SendAsync(Stream);
		}

		public void Receive(APacket packet) {
			packet.Receive(Stream);
		}

		public SubPacket Receive<SubPacket>() where SubPacket: APacket, new() {
			SubPacket packet = new SubPacket();
			Receive(packet);
			return packet;
		}

		public async Task ReceiveAsync(APacket packet) {
			await packet.ReceiveAsync(Stream);
		}

		public async Task<SubPacket> ReceiveAsync<SubPacket>() where SubPacket: APacket, new() {
			SubPacket packet = new SubPacket();
			await ReceiveAsync(packet);
			return packet;
		}

		protected virtual void OnStart() { }
		protected virtual async Task Run() { }
		protected virtual void OnClose() { }
		protected virtual void OnDisconnect() { }
		protected virtual void OnReceive() { }
	}
}