echo on
REM Run the msi installer and log the results
REM 
REM
echo "Run msi installer with log file"
cd "..\bin\release\en-us"
copy nul > logMsi.log
dir
msiexec /i "hoReverseX64.msi" /l*v "logMsi.log"
dir
cd "..\..\..\Scripts"
dir