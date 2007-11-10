// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

#include "StdAfx.h"
#include "FastShape.h"
#include "FastPage.h"
#include <vcclr.h>

#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{

			myCALLBACK fnGetDataPage;

			byte* GetPage(byte** dtpages, int smallIndex, int targetShapeIndex)
			{
				byte* dtpage=dtpages[smallIndex];
				if (dtpage==NULL)
				{
					dtpage=fnGetDataPage(smallIndex, targetShapeIndex, dtpages);
				}
				return dtpage;
			}
		}
	}
}

#pragma managed

using namespace Zamboch::Cube21;
using namespace Zamboch::Cube21::Ranking;
using namespace System::Collections::Generic;
using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::IO;
using namespace System::Threading;

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{

			public delegate byte* myCallback(int smallIndex, int targetShapeIndex, byte** dtpages);

			static byte* GetDataPage(int smallIndex, int targetShapeIndex, byte** dtpages)
			{
				byte* dtpage;
				FastShape^ targetShape=dynamic_cast<FastShape^>(DatabaseManager::GetShapeLoader(targetShapeIndex));
				FastPage^ p=dynamic_cast<FastPage^>(DatabaseManager::GetPageLoader(targetShape,smallIndex));
				dtpage = p->dataPtr;
				dtpages[smallIndex] = dtpage;
				return dtpage;
			}

			static GCHandle gch;

			static FastShape::FastShape()
			{
				myCallback^ fp=gcnew myCallback(GetDataPage);
				gch = GCHandle::Alloc(fp);
				fnGetDataPage = static_cast<myCALLBACK>(Marshal::GetFunctionPointerForDelegate(fp).ToPointer());
				loadedShapes = gcnew List<FastShape^>;
			}


			FastShape::FastShape(Zamboch::Cube21::NormalShape^ shape)
				: ShapeLoader(shape)
			{
				fileHandle=INVALID_HANDLE_VALUE;
			}

			FastShape::~FastShape()
			{
				Close();
			}

			void FastShape::LoadInternal()
			{
				Console::WriteLine("Loading shape {0:00}", ShapeIndex);

				String^ dir=System::IO::Path::GetDirectoryName(FileName);
				if (!Directory::Exists(dir))
				{
					Directory::CreateDirectory(dir);
				}

				IntPtr fileName=Marshal::StringToHGlobalUni(FileName);
				try
				{
					//SYSTEM_INFO i;
					//GetSystemInfo(&i);

					fileHandle = CreateFile((LPCWSTR)fileName.ToPointer(),
						GENERIC_READ|GENERIC_WRITE, 
						FILE_SHARE_READ /*| FILE_SHARE_WRITE*/, 
						NULL, OPEN_ALWAYS, 
						FILE_FLAG_RANDOM_ACCESS, NULL);
					if (fileHandle == INVALID_HANDLE_VALUE)
					{
						DWORD e=GetLastError();
						throw gcnew System::IO::IOException();
					}

					//prevent fragmentation
					SetFilePointer(fileHandle, PageSize*SmallPermCount, 0, FILE_BEGIN);
					SetEndOfFile(fileHandle);

					mappingHandle = CreateFileMapping(fileHandle, NULL, PAGE_READWRITE, 0, PageSize*SmallPermCount, NULL);
					if (mappingHandle == NULL)
						throw gcnew System::IO::IOException();
					
					dataPtr = (byte*)MapViewOfFile(mappingHandle, FILE_MAP_READ|FILE_MAP_WRITE, 0, 0, PageSize*SmallPermCount);
					if (dataPtr == NULL)
					{
						DWORD e=GetLastError();
						throw gcnew System::IO::IOException();
					}

					GC::AddMemoryPressure(PageSize*SmallPermCount);

					//data = (IntPtr)dataPtr;
					for(int p=0;p<Shape->Pages->Count;p++)
					{
						FastPage^ fp=dynamic_cast<FastPage^>(Shape->Pages[p]->Loader);
						if (fp!=nullptr)
							fp->UpdatePointer();
					}
				}
				finally
				{
					Marshal::FreeHGlobal(fileName);
				}
			}

			void FastShape::CloseInternal()
			{
				Console::WriteLine("Closing shape {0:00}", ShapeIndex);
				BOOL res;

				res=UnmapViewOfFile(dataPtr);
				if (!res)
					throw gcnew System::IO::IOException();

				res=CloseHandle(mappingHandle);
				if (!res)
					throw gcnew System::IO::IOException();

				res=CloseHandle(fileHandle);
				if (!res)
					throw gcnew System::IO::IOException();
				
				GC::RemoveMemoryPressure(PageSize*SmallPermCount);
			}

			void FastShape::KickOld()
			{
				FastShape^ oldShape;
				while (true)
				{
					try
					{
						Monitor::Enter(loadedShapes);
						if (loadedShapes->Count <= Zamboch::Cube21::Work::DatabaseManager::maxShapesLoaded)
						{
							break;
						}
						__int64 mintime = long::MaxValue;
						oldShape = loadedShapes[0];
						for(int i=0;i<loadedShapes->Count;i++)
						{
							FastShape^ shape = loadedShapes[i];
							if (!shape->IsUsed && shape->LastTouch < mintime)
							{
								oldShape = shape;
								mintime = shape->LastTouch;
							}
						}
						if (oldShape != nullptr)
						{
							oldShape->Close();
						}
					}
					finally
					{
						Monitor::Exit(loadedShapes);
					}
				}
			}

			bool FastShape::Load()
			{
				try
				{
					Monitor::Enter(loadedShapes);
					timeStamp++;
					try
					{
						Monitor::Enter(this);
						LastTouch = timeStamp;
						loadedCount++;
						if (isLoaded)
							return false;
						else
							loadedShapes->Add(this);

						LoadInternal();
						isLoaded=true;
					}
					catch(Exception^)
					{
						loadedCount--;
						throw;
					}
					finally
					{
						Monitor::Exit(this);
					}
				}
				finally
				{
					Monitor::Exit(loadedShapes);
				}
				KickOld();
				return true;
			}

			void FastShape::Release()
			{
				try
				{
					Monitor::Enter(this);
					loadedCount--;
				}
				finally
				{
					Monitor::Exit(this);
				}
			}

			bool FastShape::Close()
			{
				try
				{
					Monitor::Enter(loadedShapes);
					try
					{
						Monitor::Enter(this);
						if (!isLoaded)
							return false;
						if (loadedCount>0)
							return false;

						CloseInternal();

						loadedCount=0;
						isLoaded=false;
						loadedShapes->Remove(this);
					}
					finally
					{
						Monitor::Exit(this);
					}
				}
				finally
				{
					Monitor::Exit(loadedShapes);
				}
				return true;
			}
		}
	}
}