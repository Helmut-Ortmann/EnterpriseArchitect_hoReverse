@echo off
echo "Deleting registry key HKCU: hoTools ProgId: hoTools.hoToolsRoot "
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\{A4BC52BA-5972-3D7A-805E-BC8B5CDEDC3B}" /f
echo "Deleting registry key HKCU: hoTools ProgId: hoTools.hoToolsGui "
reg delete HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\{82A06E9C-7568-4E4B-8D2C-A53B8D9A7272} /f
echo "Deleting registry key HKLM: hoTools ProgId: hoTools.hoToolsRoot "
reg delete HKLM\Software\Microsoft\Windows\CurrentVersion\Explorer\{661B3D5E-F2A4-385F-BCD8-6C5E8CB56929} /f
echo "Deleting registry key HKLM: hoTools ProgId: hoTools.hoToolsGui "
reg delete HKLM\Software\Microsoft\Windows\CurrentVersion\Explorer\{82A06E9C-7568-4E4B-8D2C-A53B8D9A7272} /f
