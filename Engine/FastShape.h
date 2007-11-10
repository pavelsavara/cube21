// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

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
using namespace Zamboch::Cube21::Work;
using namespace System;
using namespace System::Collections::Generic;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
			public ref class FastShape : public ShapeLoader
			{
			public:
				FastShape(Zamboch::Cube21::NormalShape^ shape);
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

				HANDLE mappingHandle;
				HANDLE fileHandle;
				byte* dataPtr;
				int loadedCount;
				bool isLoaded;
			private:
				static List<FastShape^>^ loadedShapes;
				static long timeStamp = 1;
				void LoadInternal();
				void CloseInternal();
				void KickOld();
				static FastShape();
			};
		}
	}
}