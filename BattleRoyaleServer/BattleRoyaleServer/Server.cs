using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;

namespace BattleRoyaleServer
{
    public class Server: INetEventListener
    {

        private int MAX_CONNECTED_PEERS;
        private NetPeer[] netPeers;

        private NetManager netManager;
        private NetDataWriter dataWriter = new NetDataWriter();
        private int port = 2500;


        static void Main(string[] args)
        {
            Server program = new Server();
            program.Run();
            Console.ReadKey();
        }

        public void Run()
        {
            Console.Write("Enter maximum connections: ");
            MAX_CONNECTED_PEERS = int.Parse(Console.ReadLine());
            netPeers = new NetPeer[MAX_CONNECTED_PEERS];

            netManager = new NetManager(this);

            if (netManager.Start(port)) {

                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                Console.WriteLine("Hosting server on address : "+ myIP + " : " + port);
            
            }

            PollEvents();
        }


        public void PollEvents()
        {
            while (true)
            {
                netManager.PollEvents();
                System.Threading.Thread.Sleep(50);
            }
        }
        public int GetNextFreePosition()
        {
            for(int i = 0; i < netPeers.Length; i++)
            {
                if(netPeers[i] == null)
                {
                    return i;
                }
            }

            //No available spots
            return -1;
        }

        public void SendPostion(NetDataReader dataReader)
        {
            int peerIndex = dataReader.GetInt();
            float xPos = dataReader.GetFloat();
            float yPos = dataReader.GetFloat();
            float zPos = dataReader.GetFloat();

            float xRot = dataReader.GetFloat();
            float yRot = dataReader.GetFloat();

            float xInput = dataReader.GetFloat();
            float yInput = dataReader.GetFloat();

            dataWriter.Reset();
            dataWriter.Put((int)DataType.TRANSFORM);
            dataWriter.Put(peerIndex);
            dataWriter.Put(xPos);
            dataWriter.Put(yPos);
            dataWriter.Put(zPos);

            dataWriter.Put(xRot);
            dataWriter.Put(yRot);

            dataWriter.Put(xInput);
            dataWriter.Put(yInput);

            netManager.SendToAll(dataWriter, DeliveryMethod.Sequenced);
        }

        public int GetPeerArrrayPosition(NetPeer peer)
        {
            for (int i = 0; i < netPeers.Length; i++) { 
                if(peer == netPeers[i])
                {
                    return i;
                }
            }

            return -1;
        }

        public void StartGame()
        {
            dataWriter.Reset();
            dataWriter.Put((int) DataType.START_GAME);
            netManager.SendToAll(dataWriter, DeliveryMethod.Sequenced);
        }

        public void SendDeathInfo(NetPeer peer, NetDataReader reader)
        {
            int killedByIndex = reader.GetInt();
            float pushbackForce = reader.GetFloat();
            int killedPeer = GetPeerArrrayPosition(peer);

            dataWriter.Reset();
            dataWriter.Put((int) DataType.DEATH_INFO);
            dataWriter.Put(killedPeer);
            dataWriter.Put(killedByIndex);
            dataWriter.Put(pushbackForce);
            netManager.SendToAll(dataWriter, DeliveryMethod.Sequenced);

        }

        public void SendHitInfo(NetDataReader reader, NetPeer sender)
        {
            int hitPeerIndex = reader.GetInt();
            float damage = reader.GetFloat();
            float pushbackForce = reader.GetFloat();
            int senderPeerIndex = GetPeerArrrayPosition(sender);

            dataWriter.Reset();
            dataWriter.Put((int) DataType.HIT_INFO);
            dataWriter.Put(senderPeerIndex);
            dataWriter.Put(damage);
            dataWriter.Put(pushbackForce);
            netPeers[hitPeerIndex].Send(dataWriter, DeliveryMethod.Sequenced);
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (GetNextFreePosition() >= 0) {
            
                request.Accept();
                Console.WriteLine("Accepted connection request by " + request.Peer.EndPoint.Address);
            
            }
            else
            {
                Console.WriteLine("Connection declined, all spots filled");
            }
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            DataType command = (DataType)reader.GetInt();

            switch (command) {

                case DataType.TRANSFORM:
                    SendPostion(reader);
                    break;

                case DataType.DEATH_INFO:
                    SendDeathInfo(peer, reader);
                    break;
                
                case DataType.HIT_INFO:
                    SendHitInfo(reader, peer);
                    break;

                default:
                    Console.WriteLine("Unidentified packet recieved: " + command.ToString());
                    break;
            
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
   
        }

        public void OnPeerConnected(NetPeer peer)
        {
            int peerPosition = GetNextFreePosition();
            if(peerPosition < 0)
            {
                Console.WriteLine("No available positions");
                return;
            }

            dataWriter.Reset();
            dataWriter.Put((int)DataType.CONNECTION_INFO);
            dataWriter.Put(peerPosition);
            dataWriter.Put(MAX_CONNECTED_PEERS);
            netManager.SendToAll(dataWriter, DeliveryMethod.Sequenced);
            netPeers[peerPosition] = peer;

            if(GetNextFreePosition() < 0)
            {
                StartGame();
            }

        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            int position = GetPeerArrrayPosition(peer);
            if (position >= 0)
            {
                netPeers[position] = null;
                Console.WriteLine("Peer " + position + " has disconnected and their spot is empty");
            }
        }
    }

    public enum DataType
    {
        CONNECTION_INFO, TRANSFORM, HIT_INFO, DEATH_INFO, START_GAME
    }
}
