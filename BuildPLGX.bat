set Name=KeeLocker
set Homepage=https://github.com/lexx9999/KeeLocker
set ZipName=KeeLocker_new_lexx

set BuildDir=%TEMP%\Build%Name%
set SrcDir=%BuildDir%\%Name%
set LocalDir=%~dp0
set KeePass=%LocalDir%.keepassdev
set KeePassPlugins=%KeePass%\plugins
set ZipDir=%LocalDir%\%ZipName%

rmdir /S /Q "%BuildDir%"
mkdir "%BuildDir%"
mkdir "%SrcDir%"
mkdir "%SrcDir%\Forms"
mkdir "%SrcDir%\Properties"
mkdir "%SrcDir%\Resources"
mkdir "%ZipDir%"

copy "%LocalDir%\KeeLocker\Forms\*.cs" "%SrcDir%\Forms"
copy "%LocalDir%\KeeLocker\Forms\*.resx" "%SrcDir%\Forms"
copy "%LocalDir%\KeeLocker\*.cs" "%SrcDir%"

copy "%LocalDir%\KeeLocker\Properties\*.cs" "%SrcDir%\Properties"
copy "%LocalDir%\KeeLocker\Properties\Resources.*" "%SrcDir%\Properties"
copy "%LocalDir%\KeeLocker\Resources\*.png" "%SrcDir%\Resources"

copy "%LocalDir%\KeeLocker\KeeLocker.csproj" "%SrcDir%\KeeLocker.csproj"

pushd "%BuildDir%"
"%KeePass%\KeePass.exe" --debug --plgx-create "%SrcDir%" --plgx-prereq-os:Windows
popd

del /Q "%KeePassPlugins%\KeeLocker.dll"
del /Q "%KeePassPlugins%\KeeLocker.plgx"
copy "%BuildDir%\%Name%.plgx" "%KeePassPlugins%\%Name%.plgx"
copy "%BuildDir%\%Name%.plgx" "%ZipDir%\%Name%.plgx"
copy "%LocalDir%\%Name%\bin\Release\%Name%.dll" "%ZipDir%\%Name%.dll"

echo "Plugin: %Name%" > "%ZipDir%\info.txt"
echo "Homepage: %Homepage%" >> "%ZipDir%\info.txt"

set SevenZip=NUL
for %%i in ("%ProgramFiles(x86)%" "%ProgramFiles%") do if exist "%%~i\7-Zip\7z.exe" set SevenZip="%%~i\7-Zip\7z.exe"

%SevenZip% a -mx=9 "%LocalDir%\%ZipName%.zip" "%ZipDir%\%Name%.dll" "%ZipDir%\%Name%.plgx" "%ZipDir%\info.txt"

"%KeePass%\KeePass.exe" 

pause