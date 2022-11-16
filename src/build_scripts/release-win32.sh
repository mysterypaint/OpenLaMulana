# Make Windows (x86) release

if [ ! -e winbuild ]; then
  mkdir winbuild || exit 1
fi

cd ..

dotnet publish -c Release -r win-x86 -o artifacts/windows --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TieredCompilation=false || exit 1

cd build_scripts

cp ../artifacts/windows/OpenLaMulana.exe winbuild || exit 1
cp ../artifacts/windows/OpenLaMulana.pdb winbuild || exit 1
cp ../artifacts/windows/SDL2.dll winbuild || exit 1
cp -r ../artifacts/windows/Content winbuild || exit 1

cd winbuild

zip -r oll-win-x86.zip OpenLaMulana.exe OpenLaMulana.pdb SDL2.dll Content

cd ..

if [ ! -e publish ]; then
  mkdir publish || exit 1
fi

mv winbuild/oll-win-x86.zip publish/OpenLaMulana-build-win-x86.zip

rm -r winbuild
rm -r ../artifacts
