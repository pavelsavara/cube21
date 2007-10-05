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
				NormalShape^ targetShape=dynamic_cast<NormalShape^>(Database::GetShape(targetShapeIndex));
				FastPage^ p=dynamic_cast<FastPage^>(targetShape->GetPage(smallIndex));
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
			}


			FastShape::FastShape()
			{
				fileHandle=INVALID_HANDLE_VALUE;
			}

			FastShape::~FastShape()
			{
				Close();
			}

			void FastShape::Load()
			{
				try
				{
					Monitor::Enter(this);
					if (isLoaded)
						return;

					Database::OnLoadShape(this);
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
							throw gcnew System::IO::IOException();

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

						//data = (IntPtr)dataPtr;
						isLoaded=true;
						for(int p=0;p<Pages->Count;p++)
						{
							FastPage^ fp=dynamic_cast<FastPage^>(Pages[p]);
							fp->UpdatePointer();
						}
					}
					finally
					{
						Marshal::FreeHGlobal(fileName);
					}
				}
				finally
				{
					Monitor::Exit(this);
				}
			}

			void FastShape::Close()
			{
				try
				{
					Monitor::Enter(this);
					if (isLoaded)
					{
						Console::WriteLine("Closing shape {0:00}", ShapeIndex);
						BOOL res;
						isLoaded=false;

						res=UnmapViewOfFile(dataPtr);
						if (!res)
							throw gcnew System::IO::IOException();

						res=CloseHandle(mappingHandle);
						if (!res)
							throw gcnew System::IO::IOException();

						res=CloseHandle(fileHandle);
						if (!res)
							throw gcnew System::IO::IOException();
					}
				}
				finally
				{
					Monitor::Exit(this);
				}
			}

		}
	}
}