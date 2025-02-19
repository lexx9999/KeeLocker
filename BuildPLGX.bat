set Name=KeeLocker
set BuildDir=%TEMP%\Build%Name%
set SrcDir=%BuildDir%\%Name%
set LocalDir=%~dp0
set KeePass=%LocalDir%.keepassdev
set KeePassPlugins=%KeePass%\plugins

rmdir /S /Q "%BuildDir%"
mkdir "%BuildDir%"
mkdir "%SrcDir%"
mkdir "%SrcDir%\Forms"
mkdir "%SrcDir%\Properties"

copy "%LocalDir%KeeLocker\Forms\*.cs" "%SrcDir%\Forms"
copy "%LocalDir%KeeLocker\Forms\*.resx" "%SrcDir%\Forms"
copy "%LocalDir%KeeLocker\*.cs" "%SrcDir%"

copy "%LocalDir%KeeLocker\Properties\*.cs" "%SrcDir%\Properties"

copy "%LocalDir%KeeLocker\KeeLocker.csproj" "%SrcDir%\KeeLocker.csproj"

pushd "%BuildDir%"
"%KeePass%\KeePass.exe" --plgx-create "%SrcDir%" --plgx-prereq-os:Windows
popd

del /Q "%KeePassPlugins%\KeeLocker.dll"
del /Q "%KeePassPlugins%\KeeLocker.plgx"
copy "%BuildDir%\%Name%.plgx" "%KeePassPlugins%\%Name%.plgx"

"%KeePass%\KeePass.exe" 

pause