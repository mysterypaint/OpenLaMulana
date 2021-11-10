# OpenLaMulana
A C#, cross-platform port of La-Mulana Classic. Written using MonoGame in Visual Studio.

# Libs
- [MonoGame](https://www.monogame.net/) | NuGet

# Running
The game assets are not included on this repo. You must download the [original game](https://archive.org/details/La-Mulana) and move the /data/, /graphics/, /music/, and /sound/ folders to this game engine's /Content/ folder. All of the .bmps in the /graphics/ folder must also be converted from .bmp to .png. From the [original game jukebox](https://archive.org/details/la-mulana-jukebox), please also copy "m58.sgt" through "m75.sgt" to /Content/music/. Finally, you will also want to provide an "Icon.ico" and a 256x256 "Icon.bmp", which should go in the same directory as the .sln.

Alternatively for the time being, I will host the assets here: [MEGA](https://mega.nz/file/HHA0BTJK#2sbqV0yLgfJmEAh9fiqkcVKz7pmcCL4oHzSCqHSQaKc). "Icon.bmp/Icon.ico" both go in the same directory as the .sln, and the /Content/ folder should also be copied to the .sln's directory (/OpenLaMulana/Content/)

You will also need to install .NET runtime 5.0 if you don't already have it: [https://dotnet.microsoft.com/download/dotnet/5.0](https://dotnet.microsoft.com/download/dotnet/5.0)

Then, just run the .exe and you should be good to go!

# Compiling
- Clone the repo
- Copy the original game's assets (/data/, /graphics/, /music/, and /sound/) to the repo's /Content/ folder. All of the .bmps in the /graphics/ folder must also be converted from .bmp to .png.
- Rip and convert the game's .ico to .bmp, resize it to a 256x256 transparent .bmp as "Icon.bmp", then place both the resized .bmp and .ico in the same folder as the .sln
- Open the .sln in Visual Studio; Configure the .sln so that it will "Always Copy" all of the original game's assets to the new build folder.
- [Install the MGCB Editor for MonoGame](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_windows.html#install-mgcb-editor)
- Compile and Build
