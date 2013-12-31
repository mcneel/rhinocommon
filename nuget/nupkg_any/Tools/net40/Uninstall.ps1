param($installPath, $toolsPath, $package, $project)

# Get the current Post Build Event cmd
$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value

# remove lines from Post Build Event cmd that were created by installing this package
$cmdLines = $currentPostBuildCmd -split "`r`n"
$toDelete = $cmdLines | where {$_.Contains("xcopy") -and $_.Contains("Rhino3dmIO.dll") -and $_.Contains("NativeBinaries")}  
#$newCmdLines = $cmdLines -ne $toDelete
$newCmdLines = Compare-Object $cmdLines $toDelete -PassThru

$currentPostBuildCmd = $newCmdLines -join "`r`n"
$project.Properties.Item("PostBuildEvent").Value = $currentPostBuildCmd
