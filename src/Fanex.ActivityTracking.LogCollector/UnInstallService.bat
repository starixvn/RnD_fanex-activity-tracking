@ECHO OFF

REM The following directory is for .NET 2.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%

echo Installing Broadcast Command Service...
echo ---------------------------------------------------
installutil /u Fanex.ActivityTracking.LogCollector.exe /servicename="Fanex.ActivityTracking.LogCollector"
echo ---------------------------------------------------
echo Done.