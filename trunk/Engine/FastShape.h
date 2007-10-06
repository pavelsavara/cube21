#pragma once

#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			byte* GetPage(byte** dtpages, int smallIndex, int targetShapeIndex);
		}
	}
}

#pragma managed

using namespace Zamboch::Cube21::Ranking;
using namespace System;
using namespace System::Collections::Generic;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			public ref class FastShape : public NormalShape
			{
			public:
				FastShape();
				~FastShape();

				virtual bool Load() override;
				virtual void Release() override;
				virtual bool Close() override;

				virtual property bool IsLoaded
				{
					virtual bool get() override
					{
						return isLoaded;
					}
				}

				virtual property bool IsUsed
				{
					virtual bool get() override
					{
						return loadedCount>0;
					}
				}

				[System::Xml::Serialization::XmlIgnoreAttribute]
				HANDLE mappingHandle;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				HANDLE fileHandle;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				byte* dataPtr;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				int loadedCount;
				[System::Xml::Serialization::XmlIgnoreAttribute]
				bool isLoaded;
			private:
				static List<NormalShape^>^ loadedShapes;
				static long timeStamp = 1;
				void LoadInternal();
				void CloseInternal();
				void KickOld();
				static FastShape();
			};
		}
	}
}