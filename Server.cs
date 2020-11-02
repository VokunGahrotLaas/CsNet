using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using testNet.Packet;
using ClientSet = System.Collections.Generic.HashSet<testNet.Server.ServerClient>;

namespace testNet {
	public class Server {
		public class ServerClient : Client {
			public Server Server { get; }
			public ServerClient(Server server, string address, int port) : base(address, port) { Server = server; }
			public ServerClient(Server server, TcpClient tcpClient) : base(tcpClient) { Server = server; }
			
			protected override void OnDisconnect() => Server.RemoveClient(this);
			protected override void OnReceive() => Server.OnReceive(this);
		}

		private bool _disposed = false;
		protected readonly TcpListener _tcpListener;
		private Task _listen_task;
		private Task _run_task;
		private ClientSet _clients = new ClientSet();

		public int Port { get; }
		public bool IsOpen { get; private set; } = false;
		public ClientSet.Enumerator Clients => _clients.GetEnumerator();

		public Server(int port) {
			Port = port;
			_tcpListener = new TcpListener(IPAddress.Any, port);
			_tcpListener.Start();
		}

		~Server() => Dispose(false);

		public void Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			if (IsOpen)
				Close();
			if (disposing) {
				_listen_task.Dispose();
				_run_task.Dispose();
			}
		}

		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			OnStart();
			_listen_task = Listen();
			_run_task = Run();
		}

		public void Close() {
			if (!IsOpen) return;
			IsOpen = false;
			_listen_task.Wait();
			_run_task.Wait();
			ServerClient[] copy_clients = new ServerClient[_clients.Count];
			_clients.CopyTo(copy_clients);
			foreach (ServerClient client in copy_clients)
				RemoveClient(client);
			OnClose();
			_tcpListener.Stop();
		}

		private async Task Listen() {
			while (IsOpen) {
				ServerClient client = new ServerClient(this, await _tcpListener.AcceptTcpClientAsync());
				ConnectClient(client);
				await Task.Delay(1);
			}
		}

		private void ConnectClient(ServerClient client) {
			if (AcceptClient(client)) AddClient(client);
		}

		private void AddClient(ServerClient client) {
			client.Start();
			OnAddClient(client);
			_clients.Add(client);
		}

		private void RemoveClient(ServerClient client) {
			if (!_clients.Contains(client)) return;
			client.Close();
			OnRemoveClient(client);
			_clients.Remove(client);
		}

		public void SendAll(APacket packet) {
			foreach (ServerClient client in _clients)
				client.Send(packet);
		}

		public async Task SendAllAsync(APacket packet) {
			foreach (ServerClient client in _clients)
				await client.SendAsync(packet);
		}

		protected virtual void OnStart() { }
		protected virtual async Task Run() { }
		protected virtual void OnClose() { }
		protected virtual bool AcceptClient(ServerClient client) => true;
		protected virtual void OnAddClient(ServerClient client) { }
		protected virtual void OnRemoveClient(ServerClient client) { }
		protected virtual void OnReceive(ServerClient client) { }
	}
}