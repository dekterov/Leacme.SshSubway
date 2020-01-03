// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Leacme.Lib.SshSubway {

	public class Library {

		public Library() {

		}

		/// <summary>
		/// Connects to an SSH server async.
		/// /// </summary>
		/// <param name="address"></param>
		/// <param name="port"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="timeoutSeconds"></param>
		/// <returns>The established SSH connection.</returns>
		public async Task<SshClient> ConnectToSshServerWithPassword(string address, int port, string username, string password, int timeoutSeconds = 10) {
			var connectionInfo = new ConnectionInfo(address, username, new PasswordAuthenticationMethod(username, password)) { Timeout = new TimeSpan(0, 0, timeoutSeconds) };
			var client = new SshClient(connectionInfo);
			return await Task.Run(() => {
				client.Connect();
				return client;
			});
		}

	}

}