@echo off
echo Building FSharpDictionary.exe...
dotnet publish -c Release -r win-x64 --self-contained true ^
 /p:PublishSingleFile=true ^
 /p:IncludeAllContentForSelfExtract=true ^
 /p:PublishTrimmed=false
echo.
echo Build Complete!
echo bin\Release\net7.0\win-x64\publish\FSharpDictionary.exe
pause
