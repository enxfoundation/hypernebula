// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;
using System.Net.Sockets;
using HyperNebula.Common;
using HyperNebula.Crypto;
using HyperNebula.Message;

namespace HyperNebula.Network;

public class ConnectionHandler : IConnectionHandler
{
    public void HandleConnection(TcpClient client)
    {
        Log.Info("HyperNebulaListener - Accepted new connection.", "remote_node="+((IPEndPoint?)client?.Client.RemoteEndPoint)?.Address, 5);

        IPAddress? remote_ip_address = null;
        try
        {
            // Read request from remote node
            // Unless we are requesting peers, this should not exceed a few bytes.
            byte[] raw_request = new byte[HyperNebulaProtocol.MAX_REQUEST_READ_BYTES];
            int bytes_read = client.GetStream().Read(raw_request);
            raw_request = raw_request.Take(bytes_read).ToArray();
            
            byte messageType = MessageParser.DetectType(raw_request).GetByte();

            remote_ip_address = ((IPEndPoint?)client?.Client.RemoteEndPoint)?.Address;
            UInt16 remote_port = BitConverter.ToUInt16(raw_request.Skip(2).Take(2).ToArray());

            switch (messageType)
            {
                case MessageType.PING:
                    Log.Info("HyperNebula - PING received from remote node ", "remote_node="+remote_ip_address, 3);
                    
                    // Get entropy from PING
                    Tuple<byte[], UInt16> response_tuple = MessageParser.ParsePing(raw_request);
                    byte[] entropy = response_tuple.Item1;
                    remote_port = response_tuple.Item2;

                    // Send PONG
                    byte[] pong_response = Keccak.Hash256(entropy);
                    Log.Info("HyperNebula - SEND PONG", "pong="+EncodeUtil.ByteArrayToHexString(pong_response), 3);
                    client.GetStream().Write(MessageFactory.Pong(pong_response));
                    break;
                    
                case MessageType.PONG:
                    Log.Warn("HyperNebulaListener - Ignoring unexpected PONG", "remote_node="+remote_ip_address);
                    return;
                
                case MessageType.REQUEST_PEERS:
                    Log.Info("HyperNebula - REQUEST_PEERS received from remote node", "remote_node="+remote_ip_address, 5);
                    List<HNode> peer_list = HyperNebula.GetInstance().GetAllKnownPeers();
                    byte[] response = MessageFactory.RespondPeers(peer_list);
                    client.GetStream().Write(response);
                    Log.Info("HyperNebula - Sent peer_list to remote node", "remote_node="+remote_ip_address+", peer_list="+peer_list.Count, 5);
                    break;
                
                case MessageType.RESPOND_PEERS:
                    Log.Warn("HyperNebulaListener - Ignoring unexpected PONG", "remote_node="+remote_ip_address);
                    return;
                
                default:
                    Log.Warn("HyperNebulaListener - Ignoring unsupported request", "remote_node="+remote_ip_address+", message_type="+messageType);
                    return;
            }
            
            // Update last seen for remote node
            HNode remote_node = new HNode(remote_ip_address, remote_port);
            
            // No need to ping, we just received a message from this node.
            HyperNebula.GetInstance().ImportNode(remote_node, false);
            Log.Info("HyperNebula - updated last seen for remote node.", "remote_node="+remote_node.ip_address+":"+remote_node.port);
        }
        catch (Exception e)
        {
            Log.Warn("HyperNebula - Failed to handle connection",
                "e=" + e.Message + ", remote_node="+remote_ip_address);
        }
    }
}