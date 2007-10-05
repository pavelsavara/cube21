#include "StdAfx.h"
#include "FastRank.h"


#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Ranking
		{
			unsigned int FastSmallCubeRank::fromBitsCheck[smallPrime];
			int FastSmallCubeRank::fromBitsIndex[smallPrime];
			unsigned int FastSmallCubeRank::toBits[PermCount];
			unsigned int FastSmallCubeRank::toBitsEx[PermCount];

			unsigned int FastBigCubeRank::fromBitsCheck[smallPrime];
			int FastBigCubeRank::fromBitsIndex[smallPrime];
			unsigned int FastBigCubeRank::toBits[PermCount];
		}
	}
}