# Make MacOS release

if [ ! -e osxbuild ]; then
  mkdir osxbuild || exit 1
fi

cd ..

dotnet publish -c Release -r osx-x64 -o artifacts/osx --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TieredCompilation=false || exit 1

cd build_scripts

cp ../artifacts/osx/OpenLaMulana osxbuild || exit 1
cp ../artifacts/osx/OpenLaMulana.pdb osxbuild || exit 1
cp ../artifacts/osx/libopenal.1.dylib osxbuild || exit 1
cp ../artifacts/osx/libSDL2.dylib osxbuild || exit 1
cp -r ../artifacts/osx/Content osxbuild || exit 1
cp ../../LICENSE osxbuild/License.txt || exit 1

cd osxbuild

zip -r oll-osx-x64.zip OpenLaMulana OpenLaMulana.pdb libopenal.1.dylib libSDL2.dylib Content License.txt

cd ..

if [ ! -e publish ]; then
  mkdir publish || exit 1
fi

mv osxbuild/oll-osx-x64.zip publish/OpenLaMulana-build-osx-x64.zip

rm -r osxbuild
rm -r ../artifacts
