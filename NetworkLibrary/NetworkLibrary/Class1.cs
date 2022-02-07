using System;

namespace NetworkLibrary {
    public enum PacketType {
        Connect,
        Disconnect
    }

    public class Packet {
        public int ID;
    }
}
