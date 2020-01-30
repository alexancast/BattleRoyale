using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathInfoPacket : NetPacket
{
    public DeathInfoPacket(int killedByIndex, float pushbackForce)
    {
        dataWriter.Reset();
        dataWriter.Put((int) DataType.DEATH_INFO);
        dataWriter.Put(killedByIndex);
        dataWriter.Put(pushbackForce);
    }
}
