param($installPath, $toolsPath, $package, $project)

$solutionDir = [System.IO.Path]::GetDirectoryName($dte.Solution.FullName) + "\"
$path = $installPath.Replace($solutionDir, "`$(SolutionDir)")

$NativeAssembliesDir = Join-Path $path "NativeBinaries"
$x86 = $(Join-Path $NativeAssembliesDir "x86\*.*")
$x64 = $(Join-Path $NativeAssembliesDir "x64\*.*")

$postBuildCmdNewLine32 = "
xcopy /s /y `"$x86`" `"`$(TargetDir)x86\`""

$postBuildCmdNewLine64 = "
xcopy /s /y `"$x64`" `"`$(TargetDir)x64\`"`r`n"

# Get the current Post Build Event cmd
$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value

# Append our post build command if it's not already there
if (!$currentPostBuildCmd.Contains($postBuildCmdNewLine32)) {
    $project.Properties.Item("PostBuildEvent").Value += $postBuildCmdNewLine32
}
if (!$currentPostBuildCmd.Contains($postBuildCmdNewLine64)) {
    $project.Properties.Item("PostBuildEvent").Value += $postBuildCmdNewLine64
}
