using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformPacket : NetPacket
{
    public TransformPacket(int peerIndex, Vector3 position, Vector2 rotation, Vector3 inputDirection)
    {
        dataWriter.Reset();
        dataWriter.Put((int)DataType.TRANSFORM);
        dataWriter.Put(peerIndex);

        dataWriter.Put(position.x);
        dataWriter.Put(position.y);
        dataWriter.Put(position.z);

        dataWriter.Put(rotation.x);
        dataWriter.Put(rotation.y);

        dataWriter.Put(inputDirection.x);
        dataWriter.Put(inputDirection.z);

    }
}
