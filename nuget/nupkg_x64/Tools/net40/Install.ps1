param($installPath, $toolsPath, $package, $project)

$solutionDir = [System.IO.Path]::GetDirectoryName($dte.Solution.FullName) + "\"
$path = $installPath.Replace($solutionDir, "`$(SolutionDir)")

$NativeAssembliesDir = Join-Path $path "NativeBinaries"
$x64 = $(Join-Path $NativeAssembliesDir "x64\*.*")

$postBuildCmdNewLine = "
xcopy /s /y `"$x64`" `"`$(TargetDir)`""

# Get the current Post Build Event cmd
$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value

# Append our post build command if it's not already there
if (!$currentPostBuildCmd.Contains($postBuildCmdNewLine)) {
    $project.Properties.Item("PostBuildEvent").Value += $postBuildCmdNewLine
}

## The following msg is not necessary anymore because the pkg uninstaller will take care of it.
# Write-Host "--------------------------------------------------"
# Write-Host "WARNING: The line:"
# $msg = "{0}`r`n" -f $postBuildCmdNewLine
# Write-Host $msg
# Write-Host "was added to the Post Build Event of this project.  If you uninstall this package you'll have to manually delete the line from the `"Build Events -> Post-build event command line:`" section of the project file."
# Write-Host "--------------------------------------------------"
