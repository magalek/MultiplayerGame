using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Player localPlayer;

    private Dictionary<Guid, Player> playersDictionary = new Dictionary<Guid, Player>();

    private PositionPacket currentPacket;

    private Socket sender;

    private bool serverRunning = false;

    private bool shouldCloseServer = false;

    private void Awake() {
        playersDictionary[localPlayer.ID] = localPlayer;

        localPlayer.Initialize();

        StartCoroutine(ClientCoroutine());
    }

    private IEnumerator DataCoroutine() {
        while (serverRunning) {
            var lastPos = playerTransform.position;
            yield return null;
            var pos = playerTransform.position;

            if (lastPos == pos) {
                continue;
            }

            byte[] packet = new byte[sizeof(float) * 3];
            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, packet, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, packet, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, packet, 8, 4);

            sender.Send(packet);
            yield return null;
        }
    }

    private IEnumerator ClientCoroutine() {
        byte[] bytes = new byte[1024];
        IPAddress ipAddress = IPAddress.Parse("192.168.0.38");
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, 6969);

        sender = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        sender.Connect(remoteEP);

        byte[] msg = Encoding.ASCII.GetBytes($"<unity=true>&{localPlayer.ID}");

        sender.Send(msg);
        sender.Receive(bytes);

        serverRunning = true;
        StartCoroutine(DataCoroutine());
        while (!shouldCloseServer) {
            if (sender.Available > 0) {
                int bytesRec = sender.Receive(bytes);

                currentPacket = new PositionPacket(bytes, bytesRec);

                foreach (var packetPlayer in currentPacket.positions) {
                    if (!playersDictionary.ContainsKey(packetPlayer.Key) && localPlayer.ID != packetPlayer.Key) {
                        var otherPlayer = Instantiate(localPlayer);
                        otherPlayer.UpdateID(packetPlayer.Key);
                        playersDictionary[packetPlayer.Key] = otherPlayer;
                    }
                    if (localPlayer.ID != packetPlayer.Key) {
                        playersDictionary[packetPlayer.Key].UpdatePosition(packetPlayer.Value);
                    }
                }
            }
            yield return null;
        }
        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }
}

public class PositionPacket {

    public const int PLAYER_PACKET_SIZE = 28;
    public const int ID_SIZE = 16;

    public readonly int PlayerCount;

    public Dictionary<Guid, Vector3> positions;

    public PositionPacket(byte[] bytes, int bytesRec) {
        int bufferHead = 0;
        PlayerCount = bytes[bufferHead];
        bufferHead++;

        positions = new Dictionary<Guid, Vector3>(PlayerCount);

        for (int i = 0; i < PlayerCount; i++) {
            byte[] idBuffer = new byte[16];
            Buffer.BlockCopy(bytes, bufferHead, idBuffer, 0, 16);
            bufferHead += 16;
            Guid id = new Guid(idBuffer);

            float x = BitConverter.ToSingle(bytes, bufferHead);
            bufferHead += 4;
            float y = BitConverter.ToSingle(bytes, bufferHead);
            bufferHead += 4;
            float z = BitConverter.ToSingle(bytes, bufferHead);
            bufferHead += 4;

            Vector3 pos = new Vector3(x, y, z);
            positions[id] = pos;
        }
    }
}
