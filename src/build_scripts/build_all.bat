:: Execute all the scripts in this folder
@echo off 
cd /D "%~dp0"
for %%i in (*.bat) do (
 if not "%%i" == "%~0" call %%i
)