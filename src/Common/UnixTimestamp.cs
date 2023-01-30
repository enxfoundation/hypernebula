// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

namespace HyperNebula.Common;

public sealed class UnixTimestamp
{
    public static UInt64 Now(bool microtime)
    {
        if (microtime)
        {
            return (UInt64)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        return (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    public static UInt64 Now()
    {
        return (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}