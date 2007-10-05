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
			void lSave(int bigIndex, int smallIndex, int targetShapeIndex, int sourceLevel, byte** dtpages, int* cntpages);
		}
	}
}

#pragma managed

#include "FastRank.h"
#include "FastShape.h"
using namespace Zamboch::Cube21::Ranking;
using namespace System::Collections::Generic;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			public ref class FastPage : public Page
			{
			public:
				FastPage(int smallIndex, NormalShape^ shape);

				virtual bool Write(int address, int level) override;
		        virtual int GetNextAddress(int lastAddress, int level) override;
				virtual void UpdatePointer() override;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				byte* dataPtr;
			protected:
				FastPage()
				{
				}
			private:
			};
		}
	}
}