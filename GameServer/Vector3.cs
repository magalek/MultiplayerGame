using System;

public struct Vector3 {

    public static Vector3 Zero => new Vector3(0, 0, 0);

    public float x, y, z;

    public Vector3(float _x, float _y, float _z) {
        x = _x;
        y = _y;
        z = _z;
    }

    public static bool operator ==(Vector3 right, Vector3 left) {
        return left.x == right.x && left.y == right.y && left.z == right.z;
    }

    public static bool operator !=(Vector3 right, Vector3 left) {
        return !(right == left);
    }

    public static Vector3 FromBytes(byte[] bytes) {
        return new Vector3(
            BitConverter.ToSingle(bytes),
            BitConverter.ToSingle(bytes, 4),
            BitConverter.ToSingle(bytes, 8));
    }

    public byte[] ToByteArray() {
        byte[] buffer = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(x), 0, buffer, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(y), 0, buffer, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(z), 0, buffer, 8, 4);
        return buffer;
    }
}
