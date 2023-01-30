// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

namespace HyperNebula;

public class HBucket
{
    /// <summary>
    /// A list of nodes.
    /// </summary>
    public List<HNode> nodes;

    public HBucket()
    {
        this.nodes = new List<HNode>();
    }

    /// <summary>
    /// Returns whether this H-Bucket contains specified node.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>whether this H-Bucket contains specified node.</returns>
    public bool Contains(HNode node)
    {
        foreach (HNode n in nodes)
        {
            if (n.id.Equals(node.id))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Add node to this H-Bucket without checking if full.
    /// </summary>
    /// <param name="node"></param>
    /// <exception cref="Exception"></exception>
    public void AddNode(HNode node)
    {
        if (this.nodes.Contains(node))
        {
            throw new Exception("h-bucket already contains node_id "+node.id);
        }

        this.nodes.Add(node);
    }
    
    /// <summary>
    /// Removes specified node from H-Bucket, if it is found.
    /// </summary>
    /// <param name="node"></param>
    public void RemoveNode(HNode node)
    {
        if (this.Contains(node))
        {
            this.nodes.Remove(node);
        }
    }
    
    /// <summary>
    /// Returns a count of the nodes in this H-Bucket
    /// </summary>
    /// <returns>quantity of nodes in this h-bucket.</returns>
    public UInt32 CountNodes()
    {
        return (UInt32) this.nodes.Count;
    }

    public override string ToString()
    {
        string str = "{\n";
        str += "\tnode_count: "+this.nodes.Count+"\n";
        foreach (HNode node in nodes)
        {
            str += "\t"+(node.ip_address+":"+node.port)+"\n";
        }

        str += "}\n";

        return str;
    }
}