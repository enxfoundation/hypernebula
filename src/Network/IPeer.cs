// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;

namespace HyperNebula.Network;

public interface IPeer
{
	public IPAddress GetIpAddress();

	public UInt16 GetPort();
}