:: Make MacOS (x64) release

if not exist "osxbuild" mkdir "osxbuild"

cd ..

dotnet publish -c Release -r osx-x64 -o artifacts/osx --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TieredCompilation=false || exit 1

cd build_scripts

copy ..\artifacts\osx\OpenLaMulana osxbuild || exit 1
copy ..\artifacts\osx\OpenLaMulana osxbuild || exit 1
copy ..\artifacts\osx\libSDL2.dylib osxbuild || exit 1
copy ..\artifacts\osx\libopenal.1.dylib osxbuild || exit 1
xcopy ..\artifacts\osx\Content\* osxbuild\Content\* /s || exit 1
copy ..\..\LICENSE osxbuild\License.txt || exit 1

if not exist "publish" mkdir "publish"

powershell Compress-Archive -Path .\osxbuild\* -DestinationPath publish\OpenLaMulana-build-osx-x64.zip -Force

del /f /s /q osxbuild 1>nul
rmdir /s /q osxbuild

del /f /s /q ..\artifacts 1>nul
rmdir /s /q ..\artifacts

del /f /s /q ..\bin 1>nul
rmdir /s /q ..\bin

del /f /s /q ..\obj 1>nul
rmdir /s /q ..\obj
