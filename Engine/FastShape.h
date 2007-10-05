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
				virtual property String^ FileName
				{
					virtual String^ get()
					{
		                return "Cube\\Shape" + ShapeIndex.ToString("00") + ".data";
					}
				}

				virtual void Load() override;
				virtual void Close() override;

				virtual property bool IsLoaded
				{
					virtual bool get() override
					{
						return isLoaded;
					}
				}

				[System::Xml::Serialization::XmlIgnoreAttribute]
				HANDLE mappingHandle;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				HANDLE fileHandle;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				byte* dataPtr;

				[System::Xml::Serialization::XmlIgnoreAttribute]
				bool isLoaded;
			private:
				static FastShape();
			};
		}
	}
}