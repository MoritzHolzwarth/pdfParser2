
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    # Fallback to 32-bit framework
    $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    Write-Error "csc.exe not found. Is .NET Framework 4.x installed?"
    exit 1
}

$webextension = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.Extensions.dll"
if (-not (Test-Path $webextension)) {
    # Fallback to 32-bit framework
    $webextension = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Web.Extensions.dll"
}
if (-not (Test-Path $webextension)) {
    Write-Error "System.Web.Extensions.dll not found. Is .NET Framework 4.x installed?"
    exit 1
}

$sourceDir = "$PSScriptRoot\srcC#"
$sourceFiles = (Get-ChildItem -Path $sourceDir -Recurse -Filter "*.cs") | Select-Object -ExpandProperty FullName

if($sourceFiles.Length -eq 0) {
    Write-Error "No source Files!"
    exit 1
}

$fontDir = "$PSScriptRoot\fontData"
$fontFiles = (Get-ChildItem -Path $fontDir -Recurse -Filter "*.json") | Select-Object -ExpandProperty FullName

$outPath = Join-Path "$PSScriptRoot\bin\Debug\net472" "pdfParser2.exe"

$cscargs = @("/target:winexe", "/platform:anycpu", "/optimize+", "/langversion:5", "/out:`"$outPath`"") +
        ($fontFiles | ForEach-Object {"/resource:`"$_`",Font.$($_.Substring($fontDir.Length+1))"}) +
        ("/reference:`"$webextension`"") + #This is needed for the JavascriptSerialozer used to easily read/write to .json filed
        ($sourceFiles | ForEach-Object {"`"$_`""})

Write-Host "Compiler. $csc"
Write-Host "Argument count: $($cscArgs.Count)"
$cscArgs | ForEach-Object { Write-Host "  ARG: $_" }

& $csc @cscargs

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded: $OutputPath" -ForegroundColor Green
} else {
    Write-Error "Build FAILED (exit code $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}
