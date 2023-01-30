// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;
using System.Net.Sockets;
using HyperNebula.Common;

namespace HyperNebula.Network.TCP;

/// <summary>
/// TcpServer is a multi-threaded TCP listener
/// </summary>
public class NetTcpListener : INetListener
{
	private static TcpListener? listener = null;
	private static uint thread_counter = 0;
	private static uint port;

	private IConnectionHandler connection_handler;
	private TcpClient? client;
	private uint thread_id;

	private const int OFFLINE_TIMEOUTS = 5; // the number of timeouts before a client is considered offline

	public NetTcpListener(IPAddress address, UInt16 port, IConnectionHandler connection_handler)
	{
		if (this.thread_id != 0)
		{
			throw new Exception(
				"Unable to initialize TcpServer thread with different address and port."+
				" These parameters can only be specified on the main thread.");
		}

		this.connection_handler = connection_handler;
		this.thread_id          = NetTcpListener.thread_counter;
		NetTcpListener.thread_counter++;
		
		if (listener == null)
		{
			Initialize(address, port);
			NetTcpListener.port = port;
		}
	}
	
	public NetTcpListener(IConnectionHandler connection_handler)
	{
		this.connection_handler = connection_handler;
		this.thread_id          = NetTcpListener.thread_counter;
		NetTcpListener.thread_counter++;
		Log.Info("[" + this.thread_id + "] TcpServer - Thread Initialized", "", 5);
	}

	private void Initialize(IPAddress ip_address, UInt16 port)
	{
		Log.Info("NetTcpListener - Starting a new thread on port "+port, "", 5);
		NetTcpListener.listener = new TcpListener(ip_address, port);
		NetTcpListener.listener.Start();
	}

	public static void RunThread(object connection_handler)
	{
		new NetTcpListener((IConnectionHandler)connection_handler).Listen();
	}
	
	public void Listen()
	{
		if (port == 0)
		{
			throw new Exception("NetTcpListener - Unable to listen on port 0");
		}
		
		if (thread_id == 0)
		{
			Log.Info("[" + this.thread_id + "] TcpServer - Listening on port " + port, "", 6);
		}

		while (true)
		{
			try
			{
				TcpClient? client = TryAcceptConnection();

				if (client != null)
				{
					Log.Info("[" + this.thread_id + "] TcpServer - New connection", "remote_addr="+
					TcpHelper.GetRemoteAddress(client).Item1.ToString()+
					":"+TcpHelper.GetRemoteAddress(client).Item2.ToString(), 8);
					this.connection_handler.HandleConnection(client);
					Log.Info("[" + this.thread_id + "] TcpServer - connection handled", "remote_addr="+
						TcpHelper.GetRemoteAddress(client).Item1.ToString()+
						":"+TcpHelper.GetRemoteAddress(client).Item2.ToString(), 8);
					client.Close();
				}
			}
			catch (Exception e)
			{
				Log.Warn("Network Error: " + e.Message);
			}
		}
	}
	
	private TcpClient? TryAcceptConnection()
	{
		try
		{
			TcpClient client = NetTcpListener.listener.AcceptTcpClient();
			client.GetStream();
			return client;
		}
		catch (Exception e)
		{
			Log.Error("["+this.thread_id+"] TcpServer - Failed to accept connection", "e= "+e.Message);
		}

		return null;
	}
}