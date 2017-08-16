@echo off
setlocal enabledelayedexpansion

if "%1" == "" (
  echo No "copy-to-dir" param found
  goto :eof
)

for %%k in (*.sln *.cs *.csproj *.config *.cpp *.h *.vcxproj* *.txt) do (
  xcopy %%k %1\ /S /Y /Q /EXCLUDE:%~dp0\nocopy.lst
)

for /d %%f in (*) do (
 pushd %%f
 call %0 %1\%%f
 popd
)