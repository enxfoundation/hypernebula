// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;

namespace HyperNebula.Network.TCP;

using NetTcpClient = System.Net.Sockets.TcpClient;

internal static class TcpHelper
{
	public static Tuple<IPAddress, UInt16> GetRemoteAddress(NetTcpClient client)
	{
		IPAddress ip = IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
		UInt16 port = (UInt16) ((IPEndPoint)client.Client.RemoteEndPoint).Port;
		return new Tuple<IPAddress, UInt16>(ip, port);
	}
}