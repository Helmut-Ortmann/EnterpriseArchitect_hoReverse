echo on
REM Run the msi installer and log the results
REM 
REM
echo "Run msi installer with log file"
cd "..\bin\release\"
copy nul > logMsi.log
dir
msiexec /i "hoReverse.msi" /l*v "logMsi.log"
dir
cd "..\..\..\Scripts"