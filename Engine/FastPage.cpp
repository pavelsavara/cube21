#include "StdAfx.h"
#include "FastPage.h"
#include <vcclr.h>

#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			#pragma region Page operations
			bool lWrite(byte* dataPtr, int address, int level)
			{
				bool hi=((address&0x1) == 0x1);
				address>>=1;
				byte d=dataPtr[address];
				if (hi)
				{
					if ((d&0xF0)!=0x00)
					{
						return false;
					}
					d|=level<<4;
				}
				else
				{
					if ((d&0x0F)!=0x00)
				{
					return false;
				}
					d|=level;
				}
				dataPtr[address]=d;
				return true;
			}

		    int lGetNextAddress(byte* dataPtr, int address, int level)
			{
				//TODO optimize
				do 
				{
					address++;
					if (address>=FastBigCubeRank::PermCount)
						return -1;
					bool hi=((address&0x1) == 0x1);
						int ad=address>>1;
					byte d=dataPtr[ad];
					if (hi)
					{
						d&=0xF0;
						d>>=4;
					}
					else
					{
						d&=0x0F;
					}
					if (d==level)
						return address;
				}
				while(true);
			}

			void lSave(int bigIndex, int smallIndex, int targetShapeIndex, int targetLevel, byte** dtpages, int* cntpages)
			{
				byte* dtpage=GetPage(dtpages, smallIndex, targetShapeIndex);
				if (lWrite(dtpage, bigIndex, targetLevel))
				{
					cntpages[smallIndex]++;
				}
			}

			byte lTouch(int smallIndex, int targetShapeIndex)
			{
				byte* dtpages[SmallPermCount];
				memset(dtpages, 0,4*SmallPermCount);

				byte* dtpage=GetPage(dtpages, smallIndex, targetShapeIndex);
				byte d=dtpage[0] + dtpage[PageSize-1];
				return d;
			}

			#pragma endregion
		}
	}
}

#pragma managed

using namespace System::Runtime::InteropServices;
using namespace System::IO;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
		    int FastPage::GetNextAddress(int lastAddress, int level)
			{
				return lGetNextAddress(dataPtr, lastAddress, level);
			}

			bool FastPage::Write(int address, int level)
			{
				if (lWrite(dataPtr, address, level))
				{
					LevelCounts[level]++;
					return true;
				}
				return false;
			}

			FastPage::FastPage(int smallIndex, NormalShape^ shape)
				: Page(smallIndex, shape)
			{
				UpdatePointer();
			}

			void FastPage::UpdatePointer()
			{
				dataPtr=dynamic_cast<FastShape^>(Shape)->dataPtr + PageSize*SmallIndex;
			}

			byte FastPage::Touch()
			{
				return lTouch(SmallIndex, ShapeIndex);
			}

		}
	}
}