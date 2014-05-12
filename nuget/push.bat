REM this will only work if you've previously run the command
REM nuget setApiKey <api-key-for-your-account>
nuget push Rhino3dmIO.dll-AnyCPU-Windows.5.1.30000.23.nupkg -NonInteractive
nuget push Rhino3dmIO.dll-x64-Windows.5.1.30000.23.nupkg -NonInteractive
nuget push Rhino3dmIO.dll-x86-Windows.5.1.30000.23.nupkg -NonInteractive

