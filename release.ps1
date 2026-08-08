# ============================================================
# VJ-WMS Release Script (Velopack + GitHub Releases)
# Cách dùng: .\release.ps1 -Version "0.1.0" -Notes "Giao diện Phase 0"
# ============================================================
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [string]$Notes = "VJ-WMS Desktop v$Version"
)

$ErrorActionPreference = "Stop"
$ProjectDir = "D:\NgocLongJSC\VJCHEM_WH_Project"
$UIProject = "$ProjectDir\src\Desktop\VjWms.Desktop.UI"
$PublishDir = "$UIProject\bin\Release\net9.0-windows\win-x64\publish"
$VpkReleaseDir = "$ProjectDir\Releases\vpk"
$GithubRepo = "https://github.com/ThePrinceOfDemacia/VJ-WMS"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  VJ-WMS Release Builder v$Version" -ForegroundColor Cyan
Write-Host "  (Velopack + GitHub Releases)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Build Release
Write-Host "`n[1/5] Building Release..." -ForegroundColor Yellow
dotnet publish $UIProject -c Release -r win-x64 --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -p:Version=$Version
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED!" -ForegroundColor Red; exit 1 }
Write-Host "  Build OK!" -ForegroundColor Green

# Step 2: Create Velopack release package
Write-Host "`n[2/5] Packaging with Velopack..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $VpkReleaseDir | Out-Null
vpk pack --packId "VjWms" --packVersion $Version --packDir $PublishDir --mainExe "VjWms.exe" --outputDir $VpkReleaseDir --packTitle "VJ-WMS Warehouse Management"
if ($LASTEXITCODE -ne 0) { Write-Host "VPK PACK FAILED!" -ForegroundColor Red; exit 1 }
Write-Host "  Velopack OK!" -ForegroundColor Green

# Step 3: Git commit and tag
Write-Host "`n[3/5] Git commit & tag..." -ForegroundColor Yellow
Set-Location $ProjectDir
git add -A
git commit -m "Release v$Version - $Notes" --allow-empty
git tag -a "v$Version" -m "$Notes" -f
git push origin main --tags -f
Write-Host "  Git push OK!" -ForegroundColor Green

# Step 4: Create GitHub Release with Velopack assets
Write-Host "`n[4/5] Creating GitHub Release..." -ForegroundColor Yellow
$releaseFiles = Get-ChildItem "$VpkReleaseDir\*" -Include "*.nupkg","*.exe","RELEASES" | ForEach-Object { $_.FullName }
$fileArgs = $releaseFiles | ForEach-Object { "`"$_`"" }

# Delete existing release if any, then create new one
gh release delete "v$Version" --yes 2>$null
gh release create "v$Version" @releaseFiles --title "VJ-WMS v$Version" --notes "$Notes" --latest
if ($LASTEXITCODE -ne 0) { Write-Host "GH RELEASE FAILED!" -ForegroundColor Red; exit 1 }
Write-Host "  GitHub Release OK!" -ForegroundColor Green

# Step 5: Summary
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  RELEASE v$Version SUCCESSFUL!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "  GitHub: $GithubRepo/releases/tag/v$Version"
Write-Host ""
Write-Host "  CHO CHI BAN:" -ForegroundColor Cyan
Write-Host "  - Lan dau: Tai file 'VjWms-v$Version-win-Setup.exe' tu GitHub"
Write-Host "  - Lan sau: App TU DONG cap nhat khi mo len!"
Write-Host "========================================" -ForegroundColor Green
