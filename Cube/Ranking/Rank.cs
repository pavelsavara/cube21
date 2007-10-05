using System;

namespace Zamboch.Cube21.Ranking
{
    public class SimpleRank
    {
        private static void swap(byte[] s, int a, int b)
        {
            byte temp = s[a];
            s[a] = s[b];
            s[b] = temp;
        }

        public static void unrank(byte[] str, int len, int rank)
        {
            if (len > 0)
            {
                swap(str, (len - 1), (rank % len));
                unrank(str, len - 1, (rank / len));
            }
        }

        public static int rankImpl(byte[] res, byte[] input, int len)
        {
            if (len == 1) return 0;
            byte s = input[len - 1];
            swap(input, len - 1, res[len - 1]);
            swap(res, s, len - 1);
            return (s + len * rankImpl(res, input, len - 1));
        }

        public static void fillR(byte[] temp, byte[] input, int len)
        {
            for (byte i = 0; i < len; i++)
            {
                temp[input[i]] = i;
            }
        }
    }

    public class FullRank
    {
        //8!
        public const int PermCount = 40320; //40320

        //full shape 8!*8!
        public const int TotalPermCount = 40320 * 40320; // 1 625 702 400

        public static byte[] Unrank(int rank)
        {
            byte[] res = {0, 1, 2, 3, 4, 5, 6, 7};
            SimpleRank.unrank(res, 8, rank);
            return res;
        }

        public static int Rank(byte[] input)
        {
            byte[] temp = new byte[8];
            byte[] inptemp = new byte[8];
            Array.Copy(input, inptemp, 8);
            SimpleRank.fillR(temp, inptemp, 8);
            return SimpleRank.rankImpl(temp, inptemp, 8);
        }
    }
}