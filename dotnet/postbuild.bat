@echo postbuild.bat called with args: "%1"
rem use dir to figure out where this batch file is being called from
rem DIR

IF "Debug" == "%1" GOTO new_debug_copy
IF "Debug" == "%1" GOTO new_release_copy

IF "Debug" == "%1" GOTO debug_copy
IF "GH_Debug" == "%1" GOTO gh_debug_copy
IF "GH_Release" == "%1" GOTO gh_release_copy

GOTO done

:new_debug_copy

echo --- Debug build copies -----------------------------------------

set TARGETDIR=..\..\..\..\rhino4\Debug\
if exist %TARGETDIR%rhino4_d.exe GOTO copy_debug_win32
GOTO after_debug_win32
:copy_debug_win32
echo copy RhinoCommon output to %TARGETDIR%
REM The asterisk in the source filename is there so copy prints filename in output window
copy RhinoCommon.*dll %TARGETDIR%
copy RhinoCommon.*pdb %TARGETDIR%

:after_debug_win32

set TARGETDIR=..\..\..\..\rhino4\x64\Debug\
if exist %TARGETDIR%rhino5x64_d.exe GOTO copy_debug_x64
GOTO after_debug_x64
:copy_debug_x64
echo copy RhinoCommon output to %TARGETDIR%
REM The asterisk in the source filename is there so copy prints filename in output window
copy RhinoCommon.*dll %TARGETDIR%
copy RhinoCommon.*pdb %TARGETDIR%

:after_debug_x64

GOTO done

:new_release_copy
echo --- Release build copies -----------------------------------------

set TARGETDIR=..\..\..\..\rhino4\Release\
if exist %TARGETDIR%rhino4.exe GOTO copy_release_win32
GOTO after_release_win32
:copy_release_win32
echo copy RhinoCommon output to %TARGETDIR%
REM The asterisk in the source filename is there so copy prints filename in output window
copy RhinoCommon.*dll %TARGETDIR%
copy RhinoCommon.*pdb %TARGETDIR%

:after_release_win32

set TARGETDIR=..\..\..\..\rhino4\x64\Release\
if exist %TARGETDIR%rhino4.exe GOTO copy_release_x64
GOTO after_release_x64
:copy_release_x64
echo copy RhinoCommon output to %TARGETDIR%
REM The asterisk in the source filename is there so copy prints filename in output window
copy RhinoCommon.*dll %TARGETDIR%
copy RhinoCommon.*pdb %TARGETDIR%

:after_release_x64

GOTO done


:debug_copy
REM --- old stuff  -----------------------------------------
echo performing debug copy...
copy RhinoCommon.dll ..\..\..\..\rhino4\Debug\RhinoCommon.dll
rem copy RhinoCommon.xml ..\..\..\..\rhino4\Debug\RhinoCommon.xml
copy RhinoCommon.pdb ..\..\..\..\rhino4\Debug\RhinoCommon.pdb
GOTO done



REM --- GrassHopper stuff below -----------------------------------


:gh_debug_copy
@echo performing grasshopper debug copy...
copy RhinoCommon.dll ..\..\..\Bin\rh_common\RhinoCommon.dll
copy RhinoCommon.xml ..\..\..\Bin\rh_common\RhinoCommon.xml
copy RhinoCommon.pdb ..\..\..\Bin\rh_common\RhinoCommon.pdb
GOTO done

:gh_release_copy
@echo performing grasshopper release copy...
copy RhinoCommon.dll ..\..\..\Bin\rh_common\RhinoCommon.dll
copy RhinoCommon.xml ..\..\..\Bin\rh_common\RhinoCommon.xml
copy RhinoCommon.pdb ..\..\..\Bin\rh_common\RhinoCommon.pdb
GOTO done

:done
exit 0

