#pragma once

#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
		}
	}
}

#pragma managed
#include "FastPage.h"
using namespace Zamboch::Cube21::Ranking;
using namespace System::Collections::Generic;
using namespace System::Threading;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			public ref class FastGenPage : public FastPage
			{
			private:
				FastGenPage()
				{
				}
			public:
				FastGenPage(int smallIndex, NormalShape^ shape)
					: FastPage(smallIndex, shape)
				{
				}

				virtual void ExpandCubes(NormalShape^ targetShape, int sourceLevel) override
				{
					int cntpages[SmallPermCount];
					memset(cntpages, 0,4*SmallPermCount);
					int targetShapeIndex = targetShape->ShapeIndex;
					int sourceSmallIndex = SmallIndex;

					ExpandPage(sourceLevel, sourceSmallIndex, targetShapeIndex, dataPtr, cntpages);

					try
					{
						Monitor::Enter(this);

						int tgLevel=sourceLevel+1;
						for(int i=0;i<SmallPermCount;i++)
						{
							if (cntpages[i]>0)
							{
								FastPage^ p=dynamic_cast<FastPage^>(targetShape->GetPage(i));
								p->LevelCounts[tgLevel]+=cntpages[i];
							}
						}
					}
					finally
					{
						Monitor::Exit(this);
					}
				}
				void ExpandPage(int sourceLevel, int sourceSmallIndex, int targetShapeIndex, byte* dataPtr, int* cntpages);
			};
		}
	}
}
