using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class NetPacket
{
    protected NetDataWriter dataWriter = new NetDataWriter();

    public NetDataWriter GetWriter() { return dataWriter; }
}
