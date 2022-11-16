:: Make Windows (x86) release

if not exist "winbuild" mkdir "winbuild"

cd ..

dotnet publish -c Release -r win-x86 -o artifacts/windows --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TieredCompilation=false || exit 1

cd build_scripts

copy ..\artifacts\windows\OpenLaMulana.exe winbuild || exit 1
copy ..\artifacts\windows\OpenLaMulana.pdb winbuild || exit 1
copy ..\artifacts\windows\SDL2.dll winbuild || exit 1
copy ..\artifacts\windows\soft_oal.dll winbuild || exit 1
xcopy ..\artifacts\windows\Content\* winbuild\Content\* /s || exit 1
copy ..\..\LICENSE winbuild\License.txt || exit 1

if not exist "publish" mkdir "publish"

powershell Compress-Archive -Path .\winbuild\* -DestinationPath publish\OpenLaMulana-build-win-x86.zip -Force

del /f /s /q winbuild 1>nul
rmdir /s /q winbuild

del /f /s /q ..\artifacts 1>nul
rmdir /s /q ..\artifacts

del /f /s /q ..\bin 1>nul
rmdir /s /q ..\bin

del /f /s /q ..\obj 1>nul
rmdir /s /q ..\obj
