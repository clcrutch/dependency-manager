param (
    [Parameter(Mandatory=$true)]
    [string]$URL
)

# Get all releases
$releases = ConvertFrom-Json (Invoke-WebRequest "https://api.github.com/repos/clcrutch/dependency-manager/releases").Content

# Get the latest release
$maxDate = ($releases.published_at | Measure-Object -Maximum).Maximum
$latestRelease = $releases | Where-Object { $_.published_at -eq $maxDate }

# Get the Windows Asset
$windowsAsset = $latestRelease.assets | Where-Object { $_.name -eq "win7-x64.zip" }

$tempDirectory = [System.IO.Path]::GetTempPath()
$tempFolder = "Clcrutch.DependencyManager"
$tempPath = Join-Path $tempDirectory $tempFolder

$windowsAssetFolder = [System.IO.Path]::GetFileNameWithoutExtension($windowsAsset.name)

if (Test-Path $tempPath) {
    Remove-Item $tempPath -Force -Recurse | Out-Null
}
New-Item -Path $tempDirectory -Name $tempFolder -ItemType Directory | Out-Null

Push-Location $tempPath

# Get the executable
Invoke-WebRequest $windowsAsset.browser_download_url -OutFile $windowsAsset.name | Out-Null
Expand-Archive -Path $windowsAsset.name -DestinationPath $windowsAssetFolder

Push-Location $windowsAssetFolder
$executable = [System.IO.Path]::Combine($tempPath, $windowsAssetFolder, "DependencyManager.exe")

Invoke-WebRequest $URL -OutFile "dependencies.yaml"

& $executable install

Pop-Location
Pop-Location