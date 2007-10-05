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
			public ref class DatabaseExt : public Database
			{
			public:
				static void Main()
				{
					if (CreateDatabase())
					{
						//Test.TestData();
					}
				}

				static DatabaseExt()
				{
					instance = gcnew DatabaseExt();

					array<Type^>^ types = gcnew array<Type^> {DatabaseExt::typeid, FastPage::typeid, FastGenPage::typeid, FastShape::typeid};

					databaseSerializer = gcnew XmlSerializer(DatabaseExt::typeid, types);
					Ranking::FastRank::Touch();
					Initialize();
				}

				static bool CreateDatabase()
				{
					return Database::CreateDatabase();
				}
			protected:
		        
				virtual Page^ CreatePageImpl(int smallIndex, NormalShape^ shape) override 
				{
					return gcnew FastGenPage(smallIndex, shape);
				}

				virtual NormalShape^ CreateShapeImpl() override
				{
					return gcnew FastShape();
				}
			};
		}
	}
}
