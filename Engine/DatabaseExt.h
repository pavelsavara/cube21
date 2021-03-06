// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

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
				DatabaseExt(String^ PathToDatabase)
					: DatabaseManager(PathToDatabase)
				{
					mappedFactory=false;
				}

				static void Main()
				{
					DatabaseExt^ manager;
					if (DatabaseManager::instance==nullptr)
					{
						manager = gcnew DatabaseExt("D:\\Cube21");
						manager->Initialize();
					}
					else
					{
						manager=dynamic_cast<DatabaseExt^>(DatabaseManager::instance);
					}
					if (manager->Explore())
					{
						Test::TestData(12, 10000);
					}
					DatabaseManager::CloseAll();
				}

				virtual Zamboch::Cube21::Work::PageLoader^ CreatePageLoder(Page^ page) override
				{
					if (mappedFactory)
						return gcnew FastGenPage(page);
					else
						return DatabaseManager::CreatePageLoder(page);
					
				}

				virtual Zamboch::Cube21::Work::ShapeLoader^ CreateShapeLoder(NormalShape^ shape) override
				{
					if (mappedFactory)
						return gcnew FastShape(shape);
					else
						return DatabaseManager::CreateShapeLoder(shape);
				}

				property bool IsMapped
				{
					void set(bool value)
					{
						mappedFactory=value;
					}
				}

				static property DatabaseExt^ Instance
				{
					DatabaseExt^ get()
					{
						return dynamic_cast<DatabaseExt^>(DatabaseManager::instance);
					}
				}

			private:
				bool mappedFactory;
				static DatabaseExt()
				{
					Ranking::FastRank::Touch();
				}
			};
		}
	}
}
