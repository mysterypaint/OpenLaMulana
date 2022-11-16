:: Make Linux (x64) release

if not exist "linuxbuild" mkdir "linuxbuild"

cd ..

dotnet publish -c Release -r linux-x64 -o artifacts/linux --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TieredCompilation=false || exit 1

cd build_scripts

copy ..\artifacts\linux\OpenLaMulana linuxbuild || exit 1
copy ..\artifacts\linux\OpenLaMulana linuxbuild || exit 1
copy ..\artifacts\linux\libSDL2-2.0.so.0 linuxbuild || exit 1
copy ..\artifacts\linux\libopenal.so.1 linuxbuild || exit 1
xcopy ..\artifacts\linux\Content\* linuxbuild\Content\* /s || exit 1
copy ..\..\LICENSE linuxbuild\License.txt || exit 1

if not exist "publish" mkdir "publish"

powershell Compress-Archive -Path .\linuxbuild\* -DestinationPath publish\OpenLaMulana-build-linux-x64.zip -Force

del /f /s /q linuxbuild 1>nul
rmdir /s /q linuxbuild

del /f /s /q ..\artifacts 1>nul
rmdir /s /q ..\artifacts

del /f /s /q ..\bin 1>nul
rmdir /s /q ..\bin

del /f /s /q ..\obj 1>nul
rmdir /s /q ..\obj
