// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using HyperNebula.Common;
using HyperNebula.Crypto;
using HyperNebula.Message;

namespace HyperNebula;

public class HyperNebula
{
    /// <summary>
    /// Local node_id
    /// </summary>
    /// <param name="local_node_id"></param>
    public UInt128 local_node_id;

    /// <summary>
    /// Local IPV4 or IPV6 address
    /// </summary>
    private IPAddress ip_address;

    /// <summary>
    /// Local port
    /// </summary>
    private UInt16 port;

    /// <summary>
    /// Dictionary of h-buckets
    /// </summary>
    public Dictionary<UInt32, HBucket> hbuckets;

    /// <summary>
    /// The total number of known peers
    /// </summary>
    private uint known_peers;

    /// <summary>
    /// Singleton
    /// </summary>
    private static readonly Lazy<HyperNebula> initializer = new(() => new HyperNebula());

    private static HyperNebula instance
    {
        get { return initializer.Value; }
    }

    public static HyperNebula GetInstance()
    {
        return HyperNebula.instance;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="local_ip"></param>
    /// <param name="local_port"></param>
    public static void Initialize(IPAddress local_ip, UInt16 local_port)
    {
        HyperNebula.GetInstance().ip_address = local_ip;
        HyperNebula.GetInstance().port = local_port;
        HyperNebula.GetInstance().local_node_id = new HNode(local_ip, local_port).id;
        HyperNebula.GetInstance().hbuckets = new Dictionary<UInt32, HBucket>();
    }

    public UInt16 GetPort()
    {
        return port;
    }

    public IPAddress GetIPAddress()
    {
        return ip_address;
    }

    public void ImportNode(HNode node)
    {
        ImportNode(node, true);
    }

    public void ImportNode(HNode node, bool enable_ping)
    {
        // First, ensure node is online
        if (enable_ping)
        {
            if (!this.Ping(node))
            {
                Log.Warn("HyperNebula - Remote node is offline", "remote_node=" + node.ip_address + ":" + node.port);
                return;
            }
        }

        // Skip import if node_id matches our node_id
        if (ByteUtil.Equals(node.id, this.local_node_id))
        {
            return;
        }

        // Next, calculate the distance of the remote node.
        UInt128 distance = HyperNebulaProtocol.CalculateDistance(local_node_id, node.id);

        if (distance.Equals(0))
        {
            // Distance of remote peer is zero, so we skip importing.
            return;
        }

        // Finally, we sort the node into the appropriate h-bucket.
        UInt32 hbucket_id = HyperNebulaProtocol.SelectNodeHBucketId(distance);

        if (!hbuckets.ContainsKey(hbucket_id))
        {
            // H-Bucket doesn't exist, so we create it
            hbuckets[hbucket_id] = new HBucket();
            Log.Info("Initialized h-bucket " + (hbucket_id), "", 5);
        }

        this.AddNodeToHBucket(hbucket_id, node);

        //Log.Info("Imported node into h-bucket "+hbucket_id, "node_id="+node.id, 5);
    }

    private void AddNodeToHBucket(UInt32 hbucket_id, HNode node)
    {
        if (hbuckets[hbucket_id].Contains(node))
        {
            // The sending node already exists in the recipient’s
            //  h-bucket, so the we move it to the tail of the list.
            hbuckets[hbucket_id].RemoveNode(node);
            hbuckets[hbucket_id].AddNode(node);
            return;
        }

        if (hbuckets[hbucket_id].CountNodes() < HyperNebulaProtocol.H)
        {
            hbuckets[hbucket_id].AddNode(node);
            Log.Info("Imported new node into h-bucket " + hbucket_id, "node_id=" + node.id);
            this.known_peers++;
            return;
        }

        // If the appropriate h-bucket is full, then we ping the 
        // h-bucket's least recently seen node. If it fails to respond,
        // then we remove it from the list and instead insert the newly 
        // discovered node at the tail of the list.
        HNode leastrecent_node = hbuckets[hbucket_id].nodes[0];

        // Ping the least recently seen node.
        if (Ping(hbuckets[hbucket_id].nodes[0]))
        {
            // Least-recently seen node responded, so it is moved
            // to the tail of the list, and the newly discovered node
            // is discarded.
            hbuckets[hbucket_id].RemoveNode(leastrecent_node);
            hbuckets[hbucket_id].AddNode(leastrecent_node);
            return;
        }

        // The least-recently seen node did not respond, so
        // instead we add the newly discovered node to the tail
        // of the list.
        hbuckets[hbucket_id].RemoveNode(leastrecent_node);
        hbuckets[hbucket_id].AddNode(node);
        Log.Info("Imported new node into h-bucket " + hbucket_id, "node_id=" + node.id);
    }

    public void DiscoverPeers()
    {
        Dictionary<UInt128, HNode> peer_list = new Dictionary<UInt128, HNode>();

        for (UInt32 j = 0; j < HyperNebulaProtocol.NODE_ID_BITS; j++)
        {
            // Ask each peer in each h-bucket for more peers
            if (hbuckets.ContainsKey(j))
            {
                // Request new peers from each node
                foreach (HNode n in hbuckets[j].nodes.ToList())
                {
                    TryImportNewPeersFromNode(n);
                }
            }
        }
    }

    private void TryImportNewPeersFromNode(HNode n)
    {
        try
        {
            ImportNewPeersFromNode(n);
        }
        catch (Exception e)
        {
            Log.Error("HyperNebula - Failed to import peers from remote node", "e="+e.Message+", remote_node="+n.ip_address+":"+n.port);
        }
    }

    private void ImportNewPeersFromNode(HNode n)
    {
        if (n.ip_address.Equals(this.ip_address) && n.port == this.port)
        {
            return; // refuse to import peers from self
        }
        Log.Info("HyperNebula - Requesting peers ", "remote_node="+n.ip_address+":"+n.port);
        
        Log.Info("Requesting peers from remote h-node", "node="+n.ip_address+":"+n.port, 5);
        List<HNode> new_peers = RequestPeersFromNode(n);
        Log.Info("Remote node responded with "+new_peers.Count+" peers", "", 5);

        // Import the new peers
        foreach (HNode new_peer in new_peers)
        {
            if (new_peer.id.GetHashCode() != this.local_node_id.GetHashCode())
            {
                if (!IsKnownPeer(new_peer))
                {
                    Log.Info("HyperNebula - New Peer discovered from remote node",
                        "remote_peer=" + new_peer.ip_address + ":" + new_peer.port, 3);
                    ImportNode(new_peer);
                }
            }
        }
    }

    private bool IsKnownPeer(HNode hnode)
    {
        // Calculate the distance of the remote node and detect h-bucket.
        UInt128 distance = HyperNebulaProtocol.CalculateDistance(local_node_id, hnode.id);
        UInt32 hbucket_id = HyperNebulaProtocol.SelectNodeHBucketId(distance);
        
        if (hbuckets.ContainsKey(hbucket_id))
        {
            foreach (HNode n in this.hbuckets[hbucket_id].nodes)
            {
                if (n.ip_address.Equals(hnode.ip_address) && n.port == hnode.port)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    public List<HNode> GetAllKnownPeers()
    {
        List<HNode> peers = new List<HNode>();
        for (UInt32 j = 0; j < HyperNebulaProtocol.NODE_ID_BITS; j++)
        {
            if (hbuckets.ContainsKey(j))
            {
                foreach (HNode n in this.hbuckets[j].nodes)
                {
                    peers.Add(n);
                }
            }
        }

        return peers;
    }

    private List<HNode> RequestPeersFromNode(HNode n)
    {
        TcpClient client = new TcpClient();
        client.SendTimeout = 1000;
        client.ReceiveTimeout = 1000;
        client.Connect(n.ip_address, n.port);

        // Send Peer Request
        client.GetStream().Write(MessageFactory.RequestPeers());
        
        // Read Response
        byte[] response = new byte[HyperNebulaProtocol.MAX_READ_BYTES];
        int bytes_read;
        try
        {
            bytes_read = client.GetStream().Read(response);
        }
        catch (Exception e)
        {
            Log.Error("HyperNebula - Failed to read response from remote node.", "e="+e.Message);
            return new List<HNode>();
        }
        List<HNode> peer_list = MessageParser.ParseRespondPeers(response.Take(bytes_read).ToArray());
        
        Log.Info("Received a peer list of "+peer_list.Count+" peers.", "remote_node="+n.ip_address+":"+n.port, 5);

        return peer_list;
    }
    
    public bool Ping(HNode node)
    {
        try
        {
            TcpClient client = new TcpClient();
            client.SendTimeout = 1000;
            client.ReceiveTimeout = 1000;
            client.Connect(node.ip_address, node.port);
            
            byte[] entropy = GenerateRandomBytes();
            
            // Send PING
            client.GetStream().Write(MessageFactory.Ping(entropy));
            Log.Info("HyperNebula - SEND PING", "pong="+EncodeUtil.ByteArrayToHexString(entropy)+", remote_node="+node.ip_address+":"+node.port, 5);

            // Calculate Expected Response
            byte[] expected_response = Keccak.Hash256(entropy);
            Log.Info("HyperNebula - Awaiting PONG", "expected_pong="+EncodeUtil.ByteArrayToHexString(expected_response), 5);

            // Read Response
            byte[] response = new byte[HyperNebulaProtocol.MAX_REQUEST_READ_BYTES];
            int bytes_read = client.GetStream().Read(response);
            if (bytes_read < 32)
            {
                Log.Warn("Invalid PONG from remote node (less than 32 bytes received)");
                return false;
            }

            byte[] pong_response = MessageParser.ParsePong(response.Take(bytes_read).ToArray());

            if (ByteUtil.Equals(expected_response, pong_response))
            {
                Log.Info("HyperNebula - Valid PONG from remote node.", "remote_node="+node.ip_address+":"+node.port, 5);
                return true;
            }

            Log.Warn("Invalid PONG from remote node (invalid hash)", "remote_node="+node.ip_address+":"+node.port);
            return false;
        }
        catch (Exception e)
        {
            Log.Warn("Remote node did not respond to PING request", "remote_node="+node.ip_address+":"+node.port+", e="+e);
            return false;
        } 
    } 
    
    private byte[] GenerateRandomBytes()
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] entropy = new byte[32];
        rng.GetBytes(entropy);
        return entropy;
    }
}