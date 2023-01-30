using System.Net;
using HyperNebula.Common;

namespace HyperNebula;

public class HyperNebulaCLI
{
    private static UInt16 ReadPort(string request_message)
    {
        while (true)
        {
            try
            {
                Console.Write(request_message);
                return UInt16.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                Log.Warn("Invalid port.");
            }
        }
    }
    
    private static IPAddress ReadIP(string request_message)
    {
        while (true)
        {
            try
            {
                Console.Write(request_message);
                return IPAddress.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                Log.Warn("Invalid IP Address.");
            }
        }
    }
    
    public static void Main(string[] args)
    {
        Console.WriteLine("HyperNebula");
        Console.WriteLine("------------------------------------");

        IPAddress bootnode_ip = ReadIP("Bootnode IP: ");
        UInt16 bootnode_port = ReadPort("Bootnode Port: ");
        
        IPAddress local_ip = ReadIP("Local IP: ");
        UInt16 local_port = ReadPort("Local Port: ");

        // Initialize HyperNebula
        HyperNebula.Initialize(local_ip, local_port);
        
        // Initialize Listener
        new Thread(HyperNebulaListener.Run).Start();
        Thread.Sleep(1000);

        if (local_port != bootnode_port)
        {
            HyperNebula.GetInstance().ImportNode(new HNode(bootnode_ip, bootnode_port));
        }
        
        Log.Info("Initialized HyperNebula");
        
        UInt64 DISPLAY_FREQ = 1;
        UInt64 LAST_DISPLAY = 0;
        while (true)
        {
            try
            {
                if(local_port != bootnode_port)
                {
                    // Boot-node does not look for new peers.
                    HyperNebula.GetInstance().DiscoverPeers();
                }
                
                // sleep 1 second between discovery
                Thread.Sleep(1000);

                // Display Connected Peers
                if (UnixTimestamp.Now() >= LAST_DISPLAY + DISPLAY_FREQ)
                {
                    Console.WriteLine();
                    List<HNode> known_peers = HyperNebula.GetInstance().GetAllKnownPeers();
                    Console.WriteLine("known_peers:\n{");
                    foreach (HNode n in known_peers)
                    {
                        Console.WriteLine("\t" + n.ip_address + ":" + n.port);
                    }

                    Console.WriteLine("}");

                    LAST_DISPLAY = UnixTimestamp.Now();
                }
            }
            catch (Exception e)
            {
                Log.Warn("HyperNebula - Exception encountered during peer discovery.", "e="+e.Message);
            }
        }
    }
}