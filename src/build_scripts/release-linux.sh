# Make MacOS release

if [ ! -e linuxbuild ]; then
  mkdir linuxbuild || exit 1
fi

cd ..

dotnet publish -c Release -r linux-x64 -o artifacts/linux --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TieredCompilation=false || exit 1

cd build_scripts

cp ../artifacts/linux/OpenLaMulana linuxbuild || exit 1
cp ../artifacts/linux/OpenLaMulana.pdb linuxbuild || exit 1
cp ../artifacts/linux/libopenal.so.1 linuxbuild || exit 1
cp ../artifacts/linux/libSDL2-2.0.so.0 linuxbuild || exit 1
cp -r ../artifacts/linux/Content linuxbuild || exit 1

cd linuxbuild

zip -r oll-linux-x64.zip OpenLaMulana OpenLaMulana.pdb libopenal.so.1 libSDL2-2.0.so.0 Content

cd ..

if [ ! -e publish ]; then
  mkdir publish || exit 1
fi

mv linuxbuild/oll-linux-x64.zip publish/OpenLaMulana-build-linux-x64.zip

rm -r linuxbuild
rm -r ../artifacts
