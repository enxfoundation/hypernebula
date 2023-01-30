// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

namespace HyperNebula.Message;

public class MessageType : IComparable
{
    public const byte PING = 0x01;
    public const byte PONG = 0x02;
    public const byte REQUEST_PEERS = 0x03;
    public const byte RESPOND_PEERS = 0x04;

    private byte message_type;

    public MessageType(byte message_type)
    {
        this.message_type = message_type;
    }

    public byte GetByte()
    {
        return message_type;
    }
    
    public int CompareTo(object? b)
    {
        if (b == null)
        {
            return 1;
        }

        if (b.GetType() != this.GetType())
        {
            return 1;
        }
        
        if (((MessageType)b).message_type == message_type)
        {
            return 0;
        }

        return 1;
    }
}