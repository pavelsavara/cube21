// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;

namespace Zamboch.Cube21.Ranking
{
    public class SmallCubeRank
    {
        //(8!/(2*2*2*2))
        public const int PermCount = 2520;

        // silver shape
        public const int TotalPermCount = 40320 * 2520; //   101 606 400

        static SmallCubeRank()
        {
            for (int index = 0; index < PermCount; index++)
            {
                byte[] bytes = SilverRank.UnRank(index);
                uint bits = BigCubeRank.GetBits(bytes);
                uint bitsex = GetBitsEx(bytes);
                uint wbits = bits;

                int h = Hash(wbits);
                while (fromBitsCheck[h] != 0)
                {
                    colisions++;
                    wbits *= 7;
                    h = Hash(wbits);
                }
                fromBitsCheck[h] = bits;
                fromBitsIndex[h] = index;
                toBits[index] = bits;
                toBitsEx[index] = bitsex;
            }
#if DEBUG
            //validate
            for (int index = 0; index < PermCount; index++)
            {
                byte[] bytes = SilverRank.UnRank(index);
                if (index != SilverRank.Rank(bytes))
                    throw new InvalidProgramCubeException();

                uint bits = Unrank(index);
                if (index != Rank(bits))
                    throw new InvalidProgramCubeException();

                uint bitsex = UnrankEx(index);
                if (index != RankEx(bitsex))
                    throw new InvalidProgramCubeException();
            }
#endif
        }

        public static uint GetBitsEx(byte[] bytes)
        {
            bool[] m = new bool[4];
            uint bits = 0;
            for (int s = 0; s < 8; s++)
            {
                byte p = bytes[s];
                bool second = m[p];
                m[p] = true;
                p <<= 1;
                if (second)
                {
                    p++;
                }
                bits <<= 4;
                bits |= p;
            }
            return bits;
        }

        public static int Rank(uint bits)
        {
            uint wbits = bits;
            int h = Hash(wbits);
            while (fromBitsCheck[h] != bits)
            {
                wbits *= 7;
                h = Hash(wbits);
            }
            return fromBitsIndex[h];
        }

        public static uint Unrank(int index)
        {
            return toBits[index];
        }

        public static int RankEx(uint bits)
        {
            uint strip = (bits & 0x66666666) >> 1;
            return Rank(strip);
        }

        public static uint UnrankEx(int index)
        {
            return toBitsEx[index];
        }

        private static int Hash(uint bits)
        {
            return (int)((bits) % smallPrime);
        }

        private const int smallPrime = 12347;
        public static int colisions;
        public static uint[] fromBitsCheck = new uint[smallPrime];
        public static int[] fromBitsIndex = new int[smallPrime];

        public static uint[] toBits = new uint[PermCount];
        public static uint[] toBitsEx = new uint[PermCount];
    }

    public class BigCubeRank
    {
        //8!
        public const int PermCount = 40320;

        //full shape 8!*8!
        public const int TotalPermCount = 40320 * 40320; // 1 625 702 400

        static BigCubeRank()
        {
            for (int index = 0; index < PermCount; index++)
            {
                byte[] bytes = FullRank.Unrank(index);
                uint bits = GetBits(bytes);
                uint wbits = bits;

                int h = Hash(wbits);
                while (fromBitsCheck[h] != 0)
                {
                    colisions++;
                    wbits *= 7;
                    h = Hash(wbits);
                }
                fromBitsCheck[h] = bits;
                fromBitsIndex[h] = index;
                toBits[index] = bits;
            }

#if DEBUG
            //validate
            for (int index = 0; index < PermCount; index++)
            {
                byte[] bytes = FullRank.Unrank(index);
                if (index != FullRank.Rank(bytes))
                    throw new InvalidProgramCubeException();

                uint bits = Unrank(index);
                if (index != Rank(bits))
                    throw new InvalidProgramCubeException();
            }
#endif
        }

        public static uint GetBits(byte[] bytes)
        {
            uint bits = 0;
            for (int s = 0; s < 8; s++)
            {
                byte p = bytes[s];
                bits <<= 4;
                bits |= p;
            }
            return bits;
        }

        public static int Rank(uint bits)
        {
            uint wbits = bits;
            int h = Hash(wbits);
            while (fromBitsCheck[h] != bits)
            {
                wbits *= 7;
                h = Hash(wbits);
            }
            return fromBitsIndex[h];
        }

        public static uint Unrank(int index)
        {
            uint res = toBits[index];
            return res;
        }

        private static int Hash(uint bits)
        {
            return (int)((bits) % smallPrime);
        }

        private const int smallPrime = 161281;
        private static int colisions;
        public static uint[] fromBitsCheck = new uint[smallPrime];
        public static int[] fromBitsIndex = new int[smallPrime];

        public static uint[] toBits = new uint[PermCount];
    }
}