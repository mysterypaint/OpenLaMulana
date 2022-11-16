:: Execute all the scripts in this folder
@echo off 
for %%i in (*.bat) do (
 if not "%%i" == "%~0" call %%i
)