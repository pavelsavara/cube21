// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

#include "StdAfx.h"
#include "FastPage.h"
#include <vcclr.h>
#include <intrin.h>

#define zamoDBG 0

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
#if zamoDBG
				byte b=dataPtr[offset>>1];
#endif
				unsigned int shift=(offset & 0x7) << 2;
				unsigned int alignedOffset=(offset & 0xFFFFFFF8) >> 1;
				long* adrAligned=(long*)(alignedOffset+dataPtr);

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
#if zamoDBG
				byte d=dataPtr[offset>>1];
				if (offset&0x1)
				{
					if (b>>4!=0)
						throw 1;
					if (d>>4!=level)
						throw 1;
				}
				else
				{
					if ((b&0xF)!=0)
						throw 1;
					if ((d&0xF)!=level)
						throw 1;
				}
#endif
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

			void lSaveExplore(int bigIndex, int smallIndex, int targetShapeIndex, int targetLevel, byte** dtpages, int* cntpages)
			{
				byte* dtpage=GetPage(dtpages, smallIndex, targetShapeIndex);
				if (lWrite(dtpage, bigIndex, targetLevel))
				{
					cntpages[smallIndex]++;
				}
			}

			void lSaveFill(int bigIndex, int smallIndex, int targetShapeIndex, int targetLevel, byte** dtpages, int* cntpages)
			{
				byte* dtpage=GetPage(dtpages, smallIndex, targetShapeIndex);
				if (lWrite(dtpage, bigIndex, targetLevel))
				{
					cntpages[smallIndex+(targetLevel*SmallPermCount)]++;
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

			byte FastPage::Read(int address)
			{
				bool hi = ((address & 0x1) == 0x1);
				address >>= 1;
				byte d = dataPtr[address];
				if (hi)
				{
					return (byte)((d & 0xF0) >> 4);
				}
				else
				{
					return (byte)((d & 0x0F));
				}
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