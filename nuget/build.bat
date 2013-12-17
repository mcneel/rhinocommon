msbuild ..\c\opennurbs\zlib\zlib.vcxproj /p:Configuration=Release /p:Platform=Win32 /p:OutDir=..\..\..\Release\ /t:rebuild
msbuild ..\c\opennurbs\opennurbs_staticlib.vcxproj /p:Configuration=Release /p:Platform=Win32 /p:OutDir=..\..\Release\ /t:rebuild
msbuild ..\c\rhcommon_opennurbs.vcxproj /p:Configuration=Release /p:Platform=Win32 /p:OutDir=..\nuget\_tmp\x86\ /t:rebuild
robocopy .\_tmp\x86 .\nupkg_any\NativeBinaries\x86 rhino3dmio_native.dll
robocopy .\_tmp\x86 .\nupkg_x86\NativeBinaries\x86 rhino3dmio_native.dll

msbuild ..\c\opennurbs\zlib\zlib.vcxproj /p:Configuration=Release /p:Platform=x64 /p:OutDir=..\..\..\x64\Release\ /t:rebuild
msbuild ..\c\opennurbs\opennurbs_staticlib.vcxproj /p:Configuration=Release /p:Platform=x64 /p:OutDir=..\..\x64\Release\ /t:rebuild
msbuild ..\c\rhcommon_opennurbs.vcxproj /p:Configuration=Release /p:Platform=x64 /p:OutDir=..\nuget\_tmp\x64\ /t:rebuild
robocopy .\_tmp\x64 .\nupkg_any\NativeBinaries\x64 rhino3dmio_native.dll
robocopy .\_tmp\x64 .\nupkg_x64\NativeBinaries\x64 rhino3dmio_native.dll

msbuild ..\dotnet\Rhino3dmIO.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\nuget\_tmp\ /t:rebuild
robocopy .\_tmp .\nupkg_any\lib\net40 Rhino3dmIO.dll
robocopy .\_tmp .\nupkg_x86\lib\net40 Rhino3dmIO.dll
robocopy .\_tmp .\nupkg_x64\lib\net40 Rhino3dmIO.dll

nuget pack nupkg_any\Rhino3dmIO.dll.nuspec -NonInteractive
nuget pack nupkg_x86\Rhino3dmIO.dll.nuspec -NonInteractive
nuget pack nupkg_x64\Rhino3dmIO.dll.nuspec -NonInteractive
