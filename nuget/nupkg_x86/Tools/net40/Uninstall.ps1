param($installPath, $toolsPath, $package, $project)

# Get the current Post Build Event cmd
$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value

# remove lines from Post Build Event cmd that were created by installing this package
$cmdLines = $currentPostBuildCmd -split "`r`n"
$toDelete = $cmdLines | Where-Object {$_.StartsWith("xcopy") -and $_.Contains("Rhino3dmIO.dll-x86-Windows") -and $_.Contains("NativeBinaries")}  
$newCmdLines = $cmdLines -ne $toDelete

$currentPostBuildCmd = $newCmdLines -join "`r`n"
$project.Properties.Item("PostBuildEvent").Value = $currentPostBuildCmd
