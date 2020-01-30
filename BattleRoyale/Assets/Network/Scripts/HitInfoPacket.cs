using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class HitInfoPacket : NetPacket
{
    public HitInfoPacket(int hitPeerIndex, float damage, float pushbackForce)
    {
        dataWriter.Reset();
        dataWriter.Put((int) DataType.HIT_INFO);
        dataWriter.Put(hitPeerIndex);
        dataWriter.Put(damage);
        dataWriter.Put(pushbackForce);

    }
}
