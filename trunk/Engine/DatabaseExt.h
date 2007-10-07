#pragma once
using namespace System;
#include "FastPage.h"
#include "FastShape.h"
#include "FastGenPage.h"
using namespace System::Xml::Serialization;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			public ref class DatabaseExt : public DatabaseManager
			{
			public:
				static void Main()
				{
					DatabaseExt^ manager = gcnew DatabaseExt();
		            manager->Initialize();
					DatabaseManager::Database->IsLocal = true;
					if (manager->Explore())
					{
						//manager->FillGaps();
						//Test.TestData();
					}
				}

				virtual Zamboch::Cube21::Work::PageLoader^ CreatePageLoder(Page^ page) override
				{
					return gcnew FastGenPage(page);
				}

				virtual Zamboch::Cube21::Work::ShapeLoader^ CreateShapeLoder(NormalShape^ shape) override
				{
					return gcnew FastShape(shape);
				}
			private:
				static DatabaseExt()
				{
					Ranking::FastRank::Touch();
				}
			};
		}
	}
}
