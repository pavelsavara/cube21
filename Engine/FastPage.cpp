#include "StdAfx.h"
#include "FastPage.h"
#include <vcclr.h>
#include <intrin.h>

#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			#pragma region Page operations

			bool lWrite(byte* dataPtr, int offset, int level)
			{
				unsigned int shift=((offset&0x7))*4;
				long* adrAligned=(long*)(((offset>>1) & 0xFFFFFFFC)+dataPtr);

				long dataOrig;
				long data;
				do
				{
					dataOrig=*adrAligned;
					data=dataOrig;
					if (((data>>shift)&0xF)!=0)
						return false;
					data|=level<<shift;
				}
				while (dataOrig!=_InterlockedCompareExchange(adrAligned, data, dataOrig));
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


			byte lTouch(int smallIndex, int shapeIndex)
			{
				byte* dtpages[SmallPermCount];
				memset(dtpages, 0,4*SmallPermCount);

				byte* dtpage=GetPage(dtpages, smallIndex, shapeIndex);
				byte d=0;
				for(int address=0;address<PageSize;address+=2048)
				{
					d^=dtpage[address];
				}
				return d;
			}

			#pragma endregion
		}
	}
}

#pragma managed

using namespace System::Runtime::InteropServices;
using namespace System::IO;
using namespace Zamboch::Cube21::Work;

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
					Page->LevelCounts[level]++;
					return true;
				}
				return false;
			}

			FastPage::FastPage(Zamboch::Cube21::Page^ page)
				: PageLoader(page)
			{
				UpdatePointer();
			}

			void FastPage::UpdatePointer()
			{
				dataPtr=dynamic_cast<FastShape^>(ShapeLoader)->dataPtr + PageSize*SmallIndex;
			}

			byte FastPage::Touch()
			{
				return lTouch(SmallIndex, ShapeIndex);
			}
		}
	}
}