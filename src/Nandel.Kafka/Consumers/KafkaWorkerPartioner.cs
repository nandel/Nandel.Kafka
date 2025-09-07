using System;
using System.Text;

namespace Nandel.Kafka.Consumers;

public static class KafkaWorkerPartioner
{
    public static int GetPartition(string key, int workerCount)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var hash = Murmur2(keyBytes);
        return (int)((uint)hash % workerCount); // ensure non-negative
    }

    private static int Murmur2(byte[] data)
    {
        const uint seed = 0x9747b28c;
        const uint m = 0x5bd1e995;
        const int r = 24;

        int len = data.Length;
        uint h = seed ^ (uint)len;
        int currentIndex = 0;

        while (len >= 4)
        {
            uint k = BitConverter.ToUInt32(data, currentIndex);
            k *= m;
            k ^= k >> r;
            k *= m;

            h *= m;
            h ^= k;

            currentIndex += 4;
            len -= 4;
        }

        switch (len)
        {
            case 3: h ^= (uint)(data[currentIndex + 2] << 16); goto case 2;
            case 2: h ^= (uint)(data[currentIndex + 1] << 8); goto case 1;
            case 1:
                h ^= data[currentIndex];
                h *= m;
                break;
        }

        h ^= h >> 13;
        h *= m;
        h ^= h >> 15;

        return (int)h;
    }
}
