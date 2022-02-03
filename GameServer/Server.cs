﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


public class Server {

    public static Server Current;

    private readonly string IPAddressString;

    public object clientsLocker = new object();

    private Socket listener;

    private bool shouldRun;

    public Server(string ip) {
        IPAddressString = ip;
        shouldRun = true;
        Current = this;
    }

    private void ListenToUnityClientsAll() {
        while (UnityClient.AllClients.Count > 0) {
            lock (clientsLocker) {
                for (int i = 0; i < UnityClient.AllClients.Count; i++) {
                    if (UnityClient.AllClients[i].Socket.Poll(100, SelectMode.SelectRead)) {
                        UnityClient.AllClients[i].UpdatePosition();
                    }
                }
            }          
        }
    }

    private void OnClientAccept(IAsyncResult result) {
        var newClient = listener.EndAccept(result);
        
        byte[] bytes = new byte[1024];

        UnityClient unityClient = null;

        newClient.Poll(100, SelectMode.SelectRead);

        if (newClient.Available > 0) {
            Console.WriteLine("New client data available");
            int bytesRec = newClient.Receive(bytes);
            string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            if (data.Contains("<unity=true>")) {
                int index = data.IndexOf('&');
                string idString = data.Substring(index + 1);
                Guid id = Guid.Parse(idString);

                Console.WriteLine($"Creating Unity Client {id}");
                unityClient = new UnityClient(newClient, $"Client {UnityClient.AllClients.Count}", id);

                byte[] msg = Encoding.ASCII.GetBytes($"<connected=true>");
                newClient.Send(msg);
            }
        }
        
        if (UnityClient.AllClients.Count == 1) {
            Task.Run(() => ListenToUnityClientsAll());
        }
        Console.WriteLine($"{unityClient?.Name} -- {newClient.RemoteEndPoint} connected.");
        listener.BeginAccept(OnClientAccept, listener);
        
    }

    public void Start()  {
        IPAddress ipAddress = IPAddress.Parse(IPAddressString);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 6969);    

        try {
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
            listener.Bind(localEndPoint);  
            listener.Listen(10);  
  
            Console.WriteLine("Waiting for a connection...");

            listener.BeginAccept(OnClientAccept, listener);

            while (shouldRun)  
            {
                if (UnityClient.AllClients.Count == 0 || UnityClient.AllClients.Any(c => c.Disconnecting)) continue;
                lock (clientsLocker) {
                    SendPackets();
                }

            }
            listener.Shutdown(SocketShutdown.Both);
            listener.Close();  
        }  
        catch (Exception e)  
        {  
            Console.WriteLine(e.ToString());  
        }  
  
        Console.WriteLine("\n Press any key to continue...");  
        Console.ReadKey();  
    }

    private void SendPackets() {
        byte[] message = new byte[256];
        int bufferHead = 0;
        message[0] = (byte)UnityClient.AllClients.Count;
        bufferHead++;

        for (int i = 0; i < UnityClient.AllClients.Count; i++) {
            var client = UnityClient.AllClients[i];
            Buffer.BlockCopy(client.ID.ToByteArray(), 0, message, bufferHead, 16);
            bufferHead += 16;
            Buffer.BlockCopy(client.Position.ToByteArray(), 0, message, bufferHead, 12);
            bufferHead += 12;
        }

        for (int i = 0; i < UnityClient.AllClients.Count; i++) {
            var client = UnityClient.AllClients[i];
            try {
                client.Socket.Send(message);
            }
            catch (SocketException) {
                Console.WriteLine("SocketClosed");
                client.Close();
                return;
            }
        }
    }
}
