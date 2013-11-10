How to build, pack, and push (to nuget.org) Rhino3dmIO NuGet packages
---------------------------------------------------------------------
note: only tested in VS 2010

1) Build the Rhino3dmIO solution as described here: https://github.com/mcneel/rhinocommon/wiki/Rhino3dmIO-Toolkit-(OpenNURBS-build)
    - select the menu: Build -> Configuration Manager (and build solution)
        Active solution configuration = Release
        Active solution platform = x86
    - change Active solution platform to x64 and repeat.
    note: this creates the .Net assemblies and native dlls and copies them in the right location for both 32bit and 64bit packages. 

2) Open a command window in the directory where this file resides. (rhinocommon/nuget)

3) Edit the following files so the version number matches the version number of the Rhino3dmIO.dll assembly (this is not necessary but a good idea)
    .\nupkg_x86\Rhino3dmIO.dll.nuspec (line 5)
    .\nupkg_x86\pushToNuGet.bat (line 3 -> version number is part of the file name)
    .\nupkg_x64\Rhino3dmIO.dll.nuspec (line 5)
    .\nupkg_x64\pushToNuGet.bat (line 3 -> version number is part of the file name)

4) type nupkg_x86\makeNuGetPkg.bat and then nupkg_x64\makeNuGetPkg.bat to create the 32bit and 64bit packages respectively.

5) Add a local package source to test the packages before deploying them.  menu: Tools -> Library Package Manager -> Package Manager Settings -> Package Manager -> Package Sources.

6) type nupkg_x86\pushToNuGet.bat and then nupkg_x64\pushToNuGet.bat to update the packages (32bit and 64bit respectively) on nuget.org. 
      note: you must have the nuget API key for your account (McNeel) already on your system for this to work.  Do that by running the command:
        "nuget.exe setApiKey <your-api-key>"

