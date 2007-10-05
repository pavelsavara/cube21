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

				virtual void ExpandCubes(NormalShape^ targetShape, int sourceLevel, Page^ sourcePage) override
				{
					ExpandPage(targetShape, sourceLevel, sourcePage);
				}
				void ExpandPage(NormalShape^ targetShape, int sourceLevel, Page^ sourcePage);
			};
		}
	}
}
