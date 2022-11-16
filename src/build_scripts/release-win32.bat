cd ..
dotnet publish -c Release -r win-x86 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
pause
