echo on
REM Run the msi installer and log the results
REM 
REM
echo "Run msi installer with log file"
copy nul > logMsi.log
cd "..\bin\release\"

dir
copy nul > logMsi.log
msiexec /i "hoReverse.msi" /l*v "..\..\..\logMsi.log"
dir
cd "..\..\..\Scripts"