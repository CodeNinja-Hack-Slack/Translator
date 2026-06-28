param(
    [switch]$NoBuild,
    [switch]$Release
)

$ErrorActionPreference = "Stop"
$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ProjectDir "TranslatorApp\TranslatorApp.csproj"
$DotNetDir = "E:\Software\NET8"
$DotNetExe = Join-Path $DotNetDir "dotnet.exe"

if (-not (Test-Path -LiteralPath $DotNetExe)) {
    Write-Host "[错误] 未找到 .NET SDK: $DotNetExe" -ForegroundColor Red
    Write-Host "请安装 .NET 8 SDK 或修改 debug.ps1 中的 `$DotNetDir 路径" -ForegroundColor Yellow
    exit 1
}

$env:PATH = "$DotNetDir;$env:PATH"
$config = if ($Release) { "Release" } else { "Debug" }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Translator - 中英翻译工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not $NoBuild) {
    Write-Host "[1/2] 正在构建项目..." -ForegroundColor Green
    Write-Host "      配置: $config" -ForegroundColor Gray
    Write-Host ""

    & $DotNetExe build $ProjectFile -c $config 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n[错误] 构建失败!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host "      构建成功!" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "[1/2] 跳过构建" -ForegroundColor Yellow
}

Write-Host "[2/2] 正在启动程序..." -ForegroundColor Green
Write-Host "      托盘图标出现后即可使用" -ForegroundColor Gray
Write-Host "      快捷键: Alt+C      = 划词翻译" -ForegroundColor Gray
Write-Host "      Alt+Shift+C = 输入翻译" -ForegroundColor Gray
Write-Host "      右键托盘图标可打开设置" -ForegroundColor Gray
Write-Host "      按 Ctrl+C 终止程序`n" -ForegroundColor Gray

try {
    & $DotNetExe run --project "$ProjectFile" -c $config 2>&1
}
catch {
    # Ctrl+C or normal exit
}

Write-Host "`n[信息] 程序已退出" -ForegroundColor Yellow
