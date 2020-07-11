using System;

public static class Util
{

    public static byte[] HexToByte(string hex)
    {
        byte[] data = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            data[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return data;
    }

}