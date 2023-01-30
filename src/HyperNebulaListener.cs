// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using HyperNebula.Common;
using HyperNebula.Network;
using HyperNebula.Network.TCP;

namespace HyperNebula;

public class HyperNebulaListener
{
    public static void Run()
    {
        ConnectionHandler ch = new ConnectionHandler();
        NetTcpListener listener = new NetTcpListener(HyperNebula.GetInstance().GetIPAddress(), HyperNebula.GetInstance().GetPort(), ch);

        while (true)
        {
            try
            {
                listener.Listen();
            }
            catch (Exception e)
            {
                Log.Error("PeerDiscovery - Listener Exception Encountered", "e="+e.Message);
            }
        }
    }
}