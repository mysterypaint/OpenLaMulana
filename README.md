# OpenLaMulana
A C#, cross-platform port of La-Mulana Classic. Written using MonoGame in Visual Studio.

## Dependencies
- [MonoGame](https://www.monogame.net/)
- [.NET SDK 7.0](https://dotnet.microsoft.com/en-us/download)
- **Windows Only**: [Visual Studio 2013 Redistributable](https://aka.ms/highdpimfc2013x64enu)
  - This may only be required if [the shaders fail to compile in MGCB](https://flatredball.gitbook.io/monogame-troubleshooting/monogame-troubleshooting/building-content-content-pipeline)
- **Mac/Linux Only**: [Wine64](https://wiki.winehq.org/FAQ#Installing_Wine) (Sorry, AMD x86 macOS Users: I need to find a working solution for compiling natively. Otherwise, compile from another OS for macOS.)
- [MIDICSV](https://www.fourmilab.ch/webtools/midicsv/) (**Only for asset generation)

## Asset Preparation
### The game assets are not provided on this repo. For the time being, I will host the assets here: [MEGA](https://mega.nz/file/LKxQxbrB#MSfyuwK8seYiLz7SIG-6roB23YAcFtMKFxd-Byr4c94)
- The archive's ``Icon.bmp`` and ``Icon.ico`` files, as well as the ``/Content/`` directory, should all be copied to the ``OpenLamulana/src/`` directory.

### Otherwise, you must do the following to compile this project from source:

- Download the [original game](https://archive.org/details/La-Mulana)
- Move the ``/data/``, ``/graphics/``, and ``/sound/`` folders to ``OpenLaMulana/Content/``
- All of the .bmp files in the ``/graphics/`` folder must be converted from .bmp to .png
- All of the .sgt files [from the original game jukebox](https://archive.org/details/la-mulana-jukebox) in the ``/music/`` folder must be converted to .mid, then moved to OpenLamulana in the ``/src/Content/music/`` directory (please create the ``/music/`` directory if it does not already exist). This can be done using [DirectMusic Producer DX9](https://archive.org/details/direct-music-producer-9)
- The converted .mid files must be annotated with "Loop" and "LoopEnd" MIDI Marker Meta Messages. I provided a tool to convert them automatically:
  - **Mac/Linux**: `$ brew install midicsv`
	- Place all the converted .sgt->.mid files in ``OpenLaMulana/MIDILooper/input/``.
	- Compile ``MidiLooper.sln``
	- Execute the ``MidiLooper`` program
  - **Windows**: add both ``midicsv.exe`` and ``csvmidi.exe`` from [this archive](https://www.fourmilab.ch/webtools/midicsv/midicsv-1.1.tar.gz) to your ``PATH``, or put them in the working directory of MidiLooper
  	- The working directory would be ``OpenLaMulana/MIDILooper/bin/Debug/net7.0`` after compiling the project; Put all your MIDIs in the ``OpenLaMulana/MIDILooper/input/`` folder.)
  - The modified .mid files should now be in ``/output/``, ready to copy to the main project, in the ``/src/Content/music/`` directory.
    - Please create the ``music`` directory if it does not already exist.

- The .dls in the ``/music/`` folder must be converted to .sf2 format. This can be done with [Vienna](http://www.synthfont.com/Downloads.html)
- Finally, you will also want to provide an ``Icon.ico`` and a 256x256 ``Icon.bmp``, which should go in the same directory as the .sln, in the ``/src/`` directory.
  - You could rip and convert the game&apos;s .ico to .bmp, resize it to a 256x256 transparent .bmp as ``Icon.bmp``, then place both the resized .bmp and .ico in the same folder as the .sln.
  - MonoGame is a bit finnicky about these files: You may have the best luck grabbing an Icon.bmp from an existing MonoGame project online, then deleting+pasting its contents within an image editing program, like [GIMP](https://www.gimp.org/) or [Aseprite](https://www.aseprite.org/).
  - The modified .bmp can then be converted to .ico using a web tool like this one: https://favicon.io/favicon-converter/ (Don't forget to rename favicon.ico back to Icon.ico!)

# Compiling
- First, ensure that .NET and MonoGame are up-to-date, or set up: [Instructions here](https://docs.monogame.net/articles/getting_started/0_getting_started.html)
- Clone the repo
- Include the assets in the ``OpenLaMulana/src/`` and ``OpenLaMulana/src/Content/`` folders respectively (explained in more detail above)
- [Build the project from CLI](https://docs.monogame.net/articles/packaging_games.html)
  - Alternatively, you can use the build scripts provided in the ``/src/build_scripts/`` folder. For Mac/Linux users, ``build_all.sh`` will execute all of the shell scripts in that folder. For Windows users, launch ``build_all.bat`` instead.
  - The output directory should be at ``OpenLaMulana/src/artifacts/``, or ``OpenLaMulana/src/build_scripts/publish/``, if you ran a script to compile.
  - If you wish to compile (and test) within Visual Studio, you must first compile the assets using the MonoGame Content Builder. Typically (after installing MonoGame as an extension within Visual Studio), this can be done by double-clicking on Content.mgcb within the Solution Explorer, then compiling all the assets within the window that pops up. This must be done every time an asset is included or removed from the project.

### Special Thanks
- sinshu, MeltySynth Library - [MeltySynth](https://github.com/sinshu/meltysynth)
- TheYawningFox, Programming help - [Twitter](https://twitter.com/theyawningfox)
- worsety, Reverse Engineering original game data - [Github](https://github.com/worsety)
