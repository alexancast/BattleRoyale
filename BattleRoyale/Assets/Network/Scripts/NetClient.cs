using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using LiteNetLib.Utils;
using UnityEngine.SceneManagement;

public class NetClient : MonoBehaviour, INetEventListener
{
    [Header("Connection info")]
    [SerializeField] private int port = 2500;
    [SerializeField] private string ipAddress = "localhost";

    [Header("Objects")]
    [SerializeField] private GameObject peerPrefab;

    [SerializeField] private GameEvent gameEvent;

    private NetManager netManager;
    private Player player;
    private NetDataWriter dataWriter = new NetDataWriter();
    private int peerIndex = -1;
    private ConnectedPeer[] connectedPeers;
    private NetPeer server;
    private bool connected;
    private Character selectedCharacter;

    public static NetClient instance;

    public void Awake()
    {
        if(instance == null)
            instance = this;

        if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SetCharacter(Character character)
    {
        selectedCharacter = character;
    }

    public void SetGameEvent(GameEvent gameEvent)
    {
        this.gameEvent = gameEvent;
    }

    public Character GetCharacter() { return selectedCharacter; }

    public bool GetConnected() { return connected; }
    public ConnectedPeer[] GetConnectedPeers() { return connectedPeers; }

    public GameObject GetPeerPrefab() { return peerPrefab; }
    public void SetPlayer(Player player) { this.player = player; }

    public void Start()
    {
        netManager = new NetManager(this);
        
    }

    public void ConnectedToServer()
    {

        if (netManager.Start())
        {
            netManager.Connect(ipAddress, port, "");
        }

        StartCoroutine(PollEvents());
    }

    public void DisconnectFromServer()
    {
        netManager.Stop();
        StopCoroutine("PollEvents");
    }

    public int GetPeerIndex() { return peerIndex; }

    public IEnumerator PollEvents()
    {
        while (true)
        {
            netManager.PollEvents();
            yield return null;
        }
    }

    public void SetPeerClone(GameObject netPeer, int peerIndex) {

        connectedPeers[peerIndex] = netPeer.GetComponent<ConnectedPeer>();
    }

    public void ConnectionEvent(NetDataReader reader, NetPeer peer)
    {

        int peerIndex = reader.GetInt();
        int MAX_CONNECTED_PEERS = reader.GetInt();

        connectedPeers = new ConnectedPeer[MAX_CONNECTED_PEERS];

        if(this.peerIndex < 0)
        {
            for (int i = 0; i < peerIndex; i++)
            {
                Debug.Log("Net peer already connected on slot: " + i);
                connectedPeers[i] = Instantiate(peerPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<ConnectedPeer>();
                connectedPeers[i].SetPeerIndex(i);
            }

            this.peerIndex = peerIndex;
            server = peer;
            connected = true;

            Debug.Log("Connected to server with index: " + this.peerIndex);

        }
        else if(peerIndex <= connectedPeers.Length - 1)
        {
            connectedPeers[peerIndex] = Instantiate(peerPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<ConnectedPeer>();
            connectedPeers[peerIndex].SetPeerIndex(peerIndex);
            Debug.Log("New peer has connected");
        }

    }

    public void RecieveDamage(NetDataReader reader)
    {
        int senderPeerIndex = reader.GetInt();
        float damage = reader.GetFloat();
        float pushbackForce = reader.GetFloat();

        player.TakeDamage(connectedPeers[senderPeerIndex], damage, pushbackForce);
    }


    public void SendPacket(NetPacket packet)
    {
        if(server != null)
        {
            server.Send(packet.GetWriter(), DeliveryMethod.Sequenced);
        }
    }


    public void UpdatePosition(NetDataReader dataReader)
    {
        if (!connected)
            return;

        int peerIndex = dataReader.GetInt();

        if (peerIndex == this.peerIndex || !connectedPeers[peerIndex].GetAlive())
            return;

        float xPos = dataReader.GetFloat();
        float yPos = dataReader.GetFloat();
        float zPos = dataReader.GetFloat();

        float xRot = dataReader.GetFloat();
        float yRot = dataReader.GetFloat();

        float xInput = dataReader.GetFloat();
        float yInput = dataReader.GetFloat();

        Transform peerTransform = connectedPeers[peerIndex].transform;
        Vector3 targetPos = new Vector3(xPos, yPos, zPos);

        peerTransform.position = Vector3.Lerp(peerTransform.position, targetPos, 0.1f); //Todo: predict movement for better match with animations
        peerTransform.eulerAngles = new Vector3(0, yRot, 0);
        connectedPeers[peerIndex].RotateSpine(new Vector3(xRot, yRot, 0));
        connectedPeers[peerIndex].SetAnimation(yInput, xInput);

        //Spine rotation will work again when the animation is split so it only affects the feet probably

    }

    public void RecieveDeathInfo(NetDataReader reader)
    {
        int killedPeer = reader.GetInt();
        int killedBy = reader.GetInt();
        float pushbackForce = reader.GetFloat();

        if (killedPeer != peerIndex)
        {
            ConnectedPeer peer = connectedPeers[killedPeer];

            if(killedBy != peerIndex)
            {
                peer.Die(connectedPeers[killedBy].transform.position, pushbackForce);
            }
            else
            {
                peer.Die(player.transform.position, pushbackForce);
                player.RegisterKill();
            }
        }

        GameEvent.instance.RegisterKill(killedBy);
       
    }

    #region LiteNetLib
    public void OnConnectionRequest(ConnectionRequest request)
    {
 
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
       
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        DataType command = (DataType) reader.GetInt();

        switch (command)
        {
            case DataType.CONNECTION_INFO:
                ConnectionEvent(reader, peer);
                break;

            case DataType.TRANSFORM:
                UpdatePosition(reader);
                break;

            case DataType.HIT_INFO:
                RecieveDamage(reader);
                break;

            case DataType.DEATH_INFO:
                RecieveDeathInfo(reader);
                break;

            case DataType.START_GAME:
                SceneManager.LoadScene("HeroSelection");
                break;

            default:
                Debug.Log("Recieved unidentified packet: " + command.ToString());
                break;
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        
    }

    public void OnPeerConnected(NetPeer peer)
    {
        
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        
    }
    #endregion

}
