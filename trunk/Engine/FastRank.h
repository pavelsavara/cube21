#pragma once

#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Ranking
		{
			public class FastSmallCubeRank
			{
			public :
				//(8!/(2*2*2*2))
				static const int PermCount = 2520;              //2520

				// silver shape
				static const int TotalPermCount = 40320 * 2520; //   101 606 400

				static const int smallPrime = 12347;

				static int Rank(unsigned int bits)
				{
		            unsigned int wbits = bits;

					int h = Hash(wbits);
					while (fromBitsCheck[h] != bits)
					{
						wbits *= 7;
						h = Hash(wbits);
					}
					return fromBitsIndex[h];
				}

				__inline static unsigned int UnRank(int index)
				{
					return toBits[index];
				}

				static int RankEx(unsigned int bits)
				{
					unsigned int strip = (bits & 0x66666666) >> 1;
					return Rank(strip);
				}

				__inline static unsigned int UnRankEx(int index)
				{
		            return toBitsEx[index];
				}

				static int Hash(unsigned int bits)
				{
					return (int)((bits) % smallPrime);
				}

				static unsigned int fromBitsCheck[smallPrime];
				static int fromBitsIndex[smallPrime];
				static unsigned int toBits[PermCount];
				static unsigned int toBitsEx[PermCount];
			};

			public class FastBigCubeRank
			{
			public:
				//8!
				static const int PermCount = 40320;                   //40320

				//full shape 8!*8!
				static const int TotalPermCount = 40320 * 40320;      // 1 625 702 400

				static const int smallPrime = 161281;

				static int Rank(unsigned int bits)
				{
		            unsigned int wbits = bits;

					int h = Hash(wbits);
					while (fromBitsCheck[h] != bits)
					{
						wbits *= 7;
						h = Hash(wbits);
					}
					return fromBitsIndex[h];
				}

				__inline static unsigned int UnRank(int index)
				{
					return toBits[index];
				}

				static int Hash(unsigned int bits)
				{
					return (int)((bits) % smallPrime);
				}

				static unsigned int fromBitsCheck[smallPrime];
				static int fromBitsIndex[smallPrime];

				static unsigned int toBits[PermCount];
			};
		}
	}
}

#pragma managed

using namespace Zamboch::Cube21::Ranking;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Ranking
		{
			public ref class FastRank
			{
				#pragma region Initialize

			private:
				static FastRank()
				{
					for(int i=0;i<FastSmallCubeRank::smallPrime;i++)
					{
						FastSmallCubeRank::fromBitsCheck[i]=SmallCubeRank::fromBitsCheck[i];
						FastSmallCubeRank::fromBitsIndex[i]=SmallCubeRank::fromBitsIndex[i];
					}
					for(int i=0;i<FastSmallCubeRank::PermCount;i++)
					{
						FastSmallCubeRank::toBits[i]=SmallCubeRank::toBits[i];
						FastSmallCubeRank::toBitsEx[i]=SmallCubeRank::toBitsEx[i];
					}

					for(int i=0;i<FastBigCubeRank::smallPrime;i++)
					{
						FastBigCubeRank::fromBitsCheck[i]=BigCubeRank::fromBitsCheck[i];
						FastBigCubeRank::fromBitsIndex[i]=BigCubeRank::fromBitsIndex[i];
					}
					for(int i=0;i<FastBigCubeRank::PermCount;i++)
					{
						unsigned int val=BigCubeRank::toBits[i];
						FastBigCubeRank::toBits[i]=val;
					}
				}
			public:
				static void Touch()
				{
				}

				#pragma endregion 


				#pragma region Forward

				static unsigned int SmallUnrank(int index)
				{
					return SmallCubeRank::Unrank(index);
				}

				static int SmallRank(unsigned int bits)
				{
					return SmallCubeRank::Rank(bits);
				}

				static unsigned int BigUnrank(int index)
				{
					return BigCubeRank::Unrank(index);
				}

				static int BigRank(unsigned int bits)
				{
					return BigCubeRank::Rank(bits);
				}
				#pragma endregion 
			};
		}
	}
}
