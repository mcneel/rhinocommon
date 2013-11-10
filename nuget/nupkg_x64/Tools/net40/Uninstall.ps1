param($installPath, $toolsPath, $package, $project)

$solutionDir = [System.IO.Path]::GetDirectoryName($dte.Solution.FullName) + "\"
$path = $installPath.Replace($solutionDir, "`$(SolutionDir)")

$NativeAssembliesDir = Join-Path $path "NativeBinaries"
$x64 = $(Join-Path $NativeAssembliesDir "x64\*.*")

$postBuildCmdNewLine = "
xcopy /s /y `"$x64`" `"`$(TargetDir)`""

# Get the current Post Build Event cmd
$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value
#Write-Host $currentPostBuildCmd

# remove lines from Post Build Event cmd that were created by installing this package
$cmdLines = $currentPostBuildCmd -split "`r`n"
$toDelete = $cmdLines | Where-Object {$_.StartsWith("xcopy") -and $_.Contains("Rhino3dmIO.dll-x64-Windows") -and $_.Contains("NativeBinaries")}  
$newCmdLines = $cmdLines -ne $toDelete
$currentPostBuildCmd = $newCmdLines -join "`r`n"
#Write-Host $currentPostBuildCmd

$project.Properties.Item("PostBuildEvent").Value = $currentPostBuildCmd
