using System;
using System.Collections.Generic;
using System.Net.Sockets;


public class UnityClient {

    public readonly Socket Socket;
    public readonly string Name;
    public readonly Guid ID;

    public static List<UnityClient> AllClients = new List<UnityClient>();

    public bool Disconnecting { get; private set; }

    public bool firstPacket = true;

    public Vector3 Position { get; private set; }
    public bool changedPosition;

    private byte[] bytes = new byte[sizeof(float) * 3];

    public UnityClient(Socket handler, string name, Guid id) {
        Socket = handler;
        Name = name;
        ID = id;
        lock (Server.Current.clientsLocker) {
            AllClients.Add(this);
        }
    }

    public void UpdatePosition() {
        changedPosition = false;
        if (Socket.Available == 0) {
            return;
        }
        Socket.Receive(bytes);
        Vector3 newPosition = Vector3.FromBytes(bytes);

        if (newPosition != Position) {
            Position = newPosition;
            changedPosition = true;
        }
        changedPosition = false;
    }

    public void Close() {
        Console.WriteLine($"{Name} -- {Socket.RemoteEndPoint} disconnected.");
        Disconnecting = true;
        AllClients.Remove(this);
        Disconnecting = false;
    }
}
