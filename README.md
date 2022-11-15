# OpenLaMulana
A C#, cross-platform port of La-Mulana Classic. Written using MonoGame in Visual Studio.

## Dependencies
- [MonoGame](https://www.monogame.net/)
- [.NET SDK 7.0](https://dotnet.microsoft.com/en-us/download)
- [MIDICSV](https://www.fourmilab.ch/webtools/midicsv/) (Only for asset generation)

## Asset Preparation
### The game assets are not provided on this repo. For the time being, I will host the assets here: [MEGA](https://mega.nz/file/vCox2JQb#fidYAlPIQHK4FbV29zFDfN857zXxMen_VDV_ybkqO3w)
- The archive's ``Icon.bmp`` and ``Icon.ico`` both go in the same directory as the .sln, and the archive's ``/Content/`` folder should also be copied to the ``OpenLamulana/src/`` directory.

### Otherwise, you must do the following to compile this project from source:

- Download the [original game](https://archive.org/details/La-Mulana)
- Move the ``/data/``, ``/graphics/``, and ``/sound/`` folders to ``OpenLaMulana/Content/``
- All of the .bmp files in the ``/graphics/`` folder must be converted from .bmp to .png
- From the [original game jukebox](https://archive.org/details/la-mulana-jukebox), please also copy all of its .sgt files to ``/Content/music/``. Replace any/all conflicting files.
- All of the .sgt files must be converted to .mid. This can be done using [DirectMusic Producer DX9](https://archive.org/details/direct-music-producer-9)
- The converted .mid files must be annotated with "Loop" and "LoopEnd" MIDI Marker Meta Messages. I provided a tool to convert them automatically:
  - **Mac/Linux**: `$ brew install midicsv`
	- Compile ``MidiLooper.sln``
	- Place all the MIDIs in ``/input/``
	- Execute the ``MidiLooper`` program
  - **Windows**: add both ``midicsv.exe`` and ``csvmidi.exe`` from [this archive](https://www.fourmilab.ch/webtools/midicsv/midicsv-1.1.tar.gz) to your ``PATH``, or put them in the working directory of MidiLooper
  	- The working directory would be ``OpenLaMulana/MIDILooper/bin/Debug/net7.0`` after compiling the project; Put all your MIDIs in the /input/ folder.)
  - The modified .mid files should now be in ``/output/``, ready to copy to the main project

- The .dls in the ``/music/`` folder must be converted to .sf2 format. This can be done with [Vienna](http://www.synthfont.com/Downloads.html)
- Finally, you will also want to provide an ``Icon.ico`` and a 256x256 ``Icon.bmp``, which should go in the same directory as the .sln.

# Compiling
- First, ensure that .NET and MonoGame are up-to-date, or set up: [Instructions here](https://docs.monogame.net/articles/getting_started/0_getting_started.html)
- Clone the repo
- Include the assets in the ``OpenLaMulana/src/`` and ``OpenLaMulana/src/Content/`` folders respectively (explained in more detail above)
- Rip and convert the game&apos;s .ico to .bmp, resize it to a 256x256 transparent .bmp as ``Icon.bmp``, then place both the resized .bmp and .ico in the same folder as the .sln
- [Build the project from CLI](https://docs.monogame.net/articles/packaging_games.html)

### Special Thanks
- sinshu, MeltySynth Library - [MeltySynth](https://github.com/sinshu/meltysynth)
- TheYawningFox, Programming help - [Twitter](https://twitter.com/theyawningfox)
- worsety, Reverse Engineering original game data - [Github](https://github.com/worsety)
