set Name=KeeLocker
set Homepage=https://github.com/lexx9999/KeeLocker
set ZipName=KeeLocker_new_lexx

set BuildDir=%TEMP%\Build%Name%
set SrcDir=%BuildDir%\%Name%
set LocalDir=%~dp0
rem Use KeePass 2.54 for dev
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
rem copy "%LocalDir%\KeeLockerAgent\bin\Release\KeeLockerAgent.exe" "%SrcDir%\%Name%Agent.exe"
rem copy "%LocalDir%\KeeLockerAgent\bin\Release\KeeLockerAgent.exe.config" "%SrcDir%\%Name%Agent.exe.config"


pushd "%BuildDir%"
rem --plgx-prereq-os:Windows

"%KeePass%\KeePass.exe" --plgx-create "%SrcDir%" --plgx-prereq-kp:2.54
echo ExitCode=%ERRORLEVEL%
popd

del /Q "%KeePassPlugins%\KeeLocker.dll"
del /Q "%KeePassPlugins%\%Name%*"

copy "%BuildDir%\%Name%.plgx" "%ZipDir%\%Name%.plgx"
copy "%LocalDir%\KeeLocker\bin\Release\%Name%.dll" "%ZipDir%\%Name%.dll"
copy "%LocalDir%\KeeLockerAgent\bin\Release\KeeLockerAgent.exe" "%ZipDir%\%Name%Agent.exe"
copy "%LocalDir%\KeeLockerAgent\bin\Release\KeeLockerAgent.exe.config" "%ZipDir%\%Name%Agent.exe.config"

copy "%BuildDir%\%Name%.plgx" "%KeePassPlugins%\%Name%.plgx"
copy "%LocalDir%\KeeLockerAgent\bin\Release\KeeLockerAgent.exe" "%KeePassPlugins%\%Name%Agent.exe"
copy "%LocalDir%\KeeLockerAgent\bin\Release\KeeLockerAgent.exe.config" "%KeePassPlugins%\%Name%Agent.exe.config"

echo "Plugin: %Name%" > "%ZipDir%\info.txt"
echo "Homepage: %Homepage%" >> "%ZipDir%\info.txt"

set SevenZip=NUL
for %%i in ("%ProgramFiles(x86)%" "%ProgramFiles%") do if exist "%%~i\7-Zip\7z.exe" set SevenZip="%%~i\7-Zip\7z.exe"

"%SevenZip%" a -mx=9 "%LocalDir%\%ZipName%.zip" "%ZipDir%\*"

echo "Press return to start keepass"
pause
"%KeePass%\KeePass.exe" 

pause