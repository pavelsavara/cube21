using System;

namespace Zamboch.Cube21.Ranking
{
    public class MultiRank
    {
        protected int[] mTypeOrder;
        protected int[] mTypeCount;
        protected int mTypes;
        protected byte[] mBaseSetString;
        protected int mLength;
        protected int mMaxPotential;
        protected int mMaxTypeLength;

        public MultiRank(byte[] multiset)
        {
            mLength = multiset.Length;
            mBaseSetString = multiset;

            //count types, minchar
            int last = 0;
            mTypes = 1;
            for (int i = 0; i < multiset.Length; i++)
            {
                if (last != multiset[i])
                {
                    last = multiset[i];
                    mTypes++;
                }
            }
            //count each type
            mTypeCount = new int[mTypes];
            mTypeOrder = new int[mTypes];
            int tc = 0;
            last = multiset[0];
            for (int i = 0; i < multiset.Length; i++)
            {
                int typ = multiset[i];
                if (last != multiset[i])
                {
                    last = multiset[i];
                    tc++;
                }
                mTypeCount[typ]++;
                mTypeOrder[typ] = tc;
                if (mMaxTypeLength < mTypeCount[typ]) mMaxTypeLength = mTypeCount[typ];
            }
            ComputePotential();
        }

        public byte[] Unrank(int aSequenceNumber)
        {
            if (aSequenceNumber < 0 || aSequenceNumber >= mMaxPotential) throw new ArgumentOutOfRangeException();
            byte[] sb = new byte[mLength];
            int potential = mMaxPotential;
            int[] typeBuffer = (int[])mTypeCount.Clone();
            int l = mLength; //delka

            for (int i = 0; i < mLength; i++, l--)
            {
                int w = ((aSequenceNumber * l) / potential);

                //hledam s kde (s+types[q])<w
                int s = 0;
                byte typ;
                for (typ = 0; (s + typeBuffer[typ]) <= w; typ++) s += typeBuffer[typ];

                aSequenceNumber -= (potential * s) / l;

                potential *= typeBuffer[typ];
                potential /= l;

                typeBuffer[typ]--;
                sb[i] = typ;
            }
            return sb;
        }

        public int Rank(byte[] aMultiset)
        {
            if (aMultiset.Length != mLength) throw new ArgumentOutOfRangeException();
            int ret = 0;
            int potential = mMaxPotential;
            int[] typeBuffer = (int[])mTypeCount.Clone();
            int l = mLength;

            for (int p = 0; p < mLength - 1 && potential > 1; p++, l--)
            {
                int s = 0;
                byte typ = (aMultiset[p]);

                //hledam s kde q<typ
                for (int q = 0; q < typ; q++) s += typeBuffer[q];

                ret += (potential * s) / l;
                potential *= typeBuffer[typ];
                potential /= l;
                typeBuffer[typ]--;
            }
            return ret;
        }

        public int Length
        {
            get { return mLength; }
        }

        public int Types
        {
            get { return mTypes; }
        }

        public int Potential
        {
            get { return mMaxPotential; }
        }

        public int this[int Type]
        {
            get { return mTypeCount[Type]; }
        }

        private static decimal Factor(int f)
        {
            int res = 1;
            for (int i = 2; i <= f; i++)
            {
                res *= i;
            }
            return res;
        }

        private void ComputePotential()
        {
            //   factorial(len)
            // -------div---------
            //(factorial(inTypes[0]) * factorial(inTypes[1]) * .. * factorial(typesCount-1))
            int len = Length;
            decimal res = Factor(len);
            for (int t = 0; t < Types; t++)
            {
                res /= Factor(this[t]);
            }
            mMaxPotential = (int)res;
        }
    }

    public class SilverRank
    {
        //(8!/(2*2*2*2))
        public const int PermCount = 2520; //2520

        // silver shape
        public const int TotalPermCount = 40320 * 2520; //   101 606 400

        private static MultiRank inst = new MultiRank(new byte[] {0, 0, 1, 1, 2, 2, 3, 3});

        public static int Rank(byte[] aMultiset)
        {
            return inst.Rank(aMultiset);
        }

        public static byte[] UnRank(int aSequenceNumber)
        {
            return inst.Unrank(aSequenceNumber);
        }
    }
}