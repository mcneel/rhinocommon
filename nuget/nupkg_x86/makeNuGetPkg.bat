REM considered adding the following line to the post-build event of the Rhino3dmIO project file but not everyone who builds the project will have nuget.exe.
REM nuget pack $(SolutionDir)nuget\nupkg_$(PlatformName)\Rhino3dmIO.dll.nuspec -NonInteractive -OutputDirectory $(SolutionDir)nuget\nupkg_$(PlatformName)\
REM just pack it here instead.
nuget pack Rhino3dmIO.dll.nuspec -NonInteractive

