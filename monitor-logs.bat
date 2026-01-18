@echo off
echo ===== Sleep Journal Android Debug Monitor =====
echo Starting log monitoring...
echo.
"%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" logcat -c
"%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" logcat | findstr /i "sleepjournal AndroidRuntime FATAL mono Exception"
