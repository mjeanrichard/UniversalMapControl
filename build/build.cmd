msbuild ..\UniversalMapControl\UniversalMapControl.csproj /t:build /p:Configuration=Debug
nuget pack ..\UniversalMapControl\UniversalMapControl.nuspec -basepath ..\UniversalMapControl\bin\Debug -version %1-debug
copy UniversalMapControl.%1-debug.nupkg c:\temp\nuget