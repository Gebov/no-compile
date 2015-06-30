@echo off

rem clean
set "outputDir=%cd%\output"
rd "%outputDir%" /q /s

rem build
set "server=%cd%\output\nuget\lib\net40"
msbuild src/server/NoCompile.sln /t:Clean,Build /p:Configuration=Release,OutDir="%server%"

rem nuget
copy %cd%\nuget\no-compile.nuspec %cd%\output\nuget\no-compile.nuspec

nuget pack %cd%\output\nuget\no-compile.nuspec -OutputDirectory %cd%\output
rd %cd%\output\nuget\ /q /s
rem -Version 2.1.0

set "vsxOut=%outputDir%\vsx"
rem msbuild src/vsx/NoCompile.Vsix.sln /t:Clean,Build /p:Configuration=Release,OutDir="%vsxOut%" /toolsversion:3.5