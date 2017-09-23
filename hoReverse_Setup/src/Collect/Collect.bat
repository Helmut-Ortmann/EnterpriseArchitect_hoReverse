SET WIX=C:\Program Files (x86)\WiX Toolset v3.11\bin\heat
del *.wxs

"%WIX%" file ..\..\..\hoHistoryGui\bin\release\x86\hoHistoryGui.dll -ag -template fragment -out hoHistoryGui.wxs

"%WIX%" file ..\..\..\hoReverseGui\bin\release\x86\hoReverseGui.dll -ag -template fragment -out hoReverseGui.wxs
"%WIX%" file ..\..\..\hoReverseRoot\bin\release\x86\hoReverseRoot.dll -ag -template fragment -out hoReverseRoot.wxs

dir
