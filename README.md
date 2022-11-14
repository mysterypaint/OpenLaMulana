# OpenLaMulana
A C#, cross-platform port of La-Mulana Classic. Written using MonoGame in Visual Studio.

# Dependencies
- [MonoGame](https://www.monogame.net/)
- [.NET SDK 7.0](https://dotnet.microsoft.com/en-us/download)
# Asset Preparation
The game assets are not included on this repo. For the time being, I will host the assets here: [MEGA](https://mega.nz/file/mL4Q3QAI#nxgzOz6jjN_GyfgQk7YaHZxReqnyNl4ObmhIxl56yoE).
- The archive's "Icon.bmp/Icon.ico" both go in the same directory as the .sln, and the archive's ``/Content/`` folder should also be copied to the ``OpenLamulana/src/`` directory.

Otherwise, you must do the following to compile this project from source:

- Download the [original game](https://archive.org/details/La-Mulana)
- Move the ``/data/``, ``/graphics/``, ``/music/``, and ``/sound/`` folders to ``OpenLaMulana/Content/``
- All of the .bmp files in the ``/graphics/`` folder must be converted from .bmp to .png
- From the [original game jukebox](https://archive.org/details/la-mulana-jukebox), please also copy "m58.sgt" through "m75.sgt" to ``/Content/music/``
- All of the .sgt files must be converted to .mid. This can be done using [DirectMusic Producer DX9](https://archive.org/details/direct-music-producer-9)
- Finally, you will also want to provide an "Icon.ico" and a 256x256 "Icon.bmp", which should go in the same directory as the .sln.

Then, just run the .exe and you should be good to go!

# Compiling
- Ensure that .NET and MonoGame are up-to-date, or set up: [Instructions here](https://docs.monogame.net/articles/getting_started/0_getting_started.html)
- Clone the repo
- Include the assets in the ``OpenLaMulana/src/`` and ``OpenLaMulana/src/Content/`` folders respectively (explained in more detail above)
- Rip and convert the game's .ico to .bmp, resize it to a 256x256 transparent .bmp as "Icon.bmp", then place both the resized .bmp and .ico in the same folder as the .sln
- Open the .sln in Visual Studio and Build
