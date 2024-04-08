#Requires -Version 5

$packWorkDir = "PackResources"

if (Test-Path $packWorkDir) {
    Remove-Item $packWorkDir -Recurse }

New-Item -Name "$packWorkDir" -ItemType "directory"

New-Item -Name "$packWorkDir\win32" -ItemType "directory"
New-Item -Name "$packWorkDir\win64" -ItemType "directory"

function FetchWinCurlAndExtractLib
{
    param (
        [string] $osArchSlug,
        [string] $versionSlug,
        [bool] $onlyIfMissing = $true,
        [bool] $deleteDownloadedArchive = $true
    )

    $curlArchiveName = "curl-$versionSlug-$osArchSlug-mingw.zip"

    $usingExistingFile = $onlyIfMissing -and (Test-Path $curlArchiveName)
    
    if ($usingExistingFile)
    {
        Write-Host("$curlArchiveName already exists")
    }
    else
    {
        $url = "https://curl.se/windows/dl-$versionSlug/$filename" # e.g. "https://curl.se/windows/dl-8.7.1_7/curl-8.7.1_7-win64-mingw.zip"
        # Only versions 8.2.0 and later are available at this host

        Write-Host "$curlArchiveName does not exist; fetching from $url"

        try
        {
            Invoke-WebRequest $url -OutFile "$curlArchiveName"
            # Some versions of SChannel do not support any of the cipher suites used by this host, so this will fail with "Could not create SSL/TLS secure channel"
        }
        catch
        {
            Write-Error $_
            Write-Host "Download failed; aborting script"
            return 1
        }
    }

    Expand-Archive -Path "$curlArchiveName" -DestinationPath "." -Force
    $extractDir = [io.Path]::GetFileNameWithoutExtension($curlArchiveName)

    if (-not $usingExistingFile -and $deleteDownloadedArchive) {
        Remove-Item "$curlArchiveName" }

    if ($osArchSlug -eq "win32") {
        $libcurlDll = "libcurl.dll" }
    else {
        $libcurlDll = "libcurl-x64.dll" }

    Copy-Item -Path "$extractDir\bin\$libcurlDll" -Destination "$packWorkDir\$osArchSlug\libcurl.dll"
    Copy-Item -Path "$extractDir\bin\*.crt" -Destination "$packWorkDir\$osArchSlug\"
    Remove-Item -Recurse "$extractDir"

    return 0
}

if (FetchWinCurlAndExtractLib "win32" "8.7.1_7" -ne 0) { return }
if (FetchWinCurlAndExtractLib "win64" "8.7.1_7" -ne 0) { return }

$packArchiveName = "Resources.zip"

if (Test-Path $packArchiveName) {
    Remove-Item $packArchiveName }

$compress = @{
    Path = "$packWorkDir\win32", "$packWorkDir\win64"
    CompressionLevel = "Optimal"
    DestinationPath = $packArchiveName
}

Compress-Archive @compress

Remove-Item -Recurse "$packWorkDir"

Write-Host "$packArchiveName build finished."
