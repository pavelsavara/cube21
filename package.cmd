set VER=1.0
mkdir Cube21_%VER%
copy /y Viewer\bin\Release\Cube.dll Cube21_%VER%
copy /y Viewer\bin\Release\Engine.dll Cube21_%VER%
copy /y Viewer\bin\Release\Viewer.exe.config Cube21_%VER%
copy /y Viewer\bin\Release\Viewer.exe Cube21_%VER%
zip -mr Cube21_%VER%.zip Cube21_%VER%

