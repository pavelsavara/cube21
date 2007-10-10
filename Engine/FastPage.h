#pragma once

#pragma unmanaged

extern "C" {
	typedef byte* (__stdcall *myCALLBACK)(int smallIndex, int targetShapeIndex, byte** dtpages);
}

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
		    int lGetNextAddress(byte* dataPtr, int address, int level);
			const int SmallPermCount = 2520;
			const int BigPermCount = 40320;
			const int PageSize = BigPermCount/2;
			void lSaveExplore(int bigIndex, int smallIndex, int targetShapeIndex, int sourceLevel, byte** dtpages, int* cntpages);
			void lSaveFill(int bigIndex, int smallIndex, int targetShapeIndex, int sourceLevel, byte** dtpages, int* cntpages);
		}
	}
}

#pragma managed

#include "FastRank.h"
#include "FastShape.h"
using namespace System::Collections::Generic;
using namespace Zamboch::Cube21::Ranking;
using namespace Zamboch::Cube21::Work;
using namespace Zamboch::Cube21;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			public ref class FastPage : public PageLoader
			{
			public:
				FastPage(Zamboch::Cube21::Page^ page);

				virtual bool Write(int address, int level) override;
		        virtual int GetNextAddress(int lastAddress, int level) override;
				virtual void UpdatePointer();
				virtual byte Touch() override;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				byte* dataPtr;
			private:
			};
		}
	}
}