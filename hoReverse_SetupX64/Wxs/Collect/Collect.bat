SET WIX=C:\Program Files (x86)\WiX Toolset v3.11\bin\heat
del *.wxs

"%WIX%" file ..\..\..\hoHistoryGui\bin\release\hoHistoryGui.dll -ag -template fragment -out hoHistoryGui.wxs

"%WIX%" file ..\..\..\hoReverseGui\bin\release\hoReverseGui.dll -ag -template fragment -out hoReverseGui.wxs
"%WIX%" file ..\..\..\hoReverseRoot\bin\release\hoReverseRoot.dll -ag -template fragment -out hoReverseRoot.wxs

dir
