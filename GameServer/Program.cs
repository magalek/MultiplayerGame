using System;

namespace GameServer {
    class Program {
        static void Main(string[] args) {
            Server server = new Server("192.168.0.38");
            server.Start();
        }
    }
}
