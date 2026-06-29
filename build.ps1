param(
    [switch]$SelfContained = $true
)

$ErrorActionPreference = "Stop"
$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ProjectDir "TranslatorApp\TranslatorApp.csproj"
$DotNetDir = "E:\Software\NET8"
$DotNetExe = Join-Path $DotNetDir "dotnet.exe"
$OutputDir = Join-Path $ProjectDir "dist"

if (-not (Test-Path -LiteralPath $DotNetExe)) {
    Write-Host "[错误] 未找到 .NET SDK: $DotNetExe" -ForegroundColor Red
    exit 1
}

$env:PATH = "$DotNetDir;$env:PATH"

if (Test-Path -LiteralPath "bin\Debug\net8.0-windows\TranslatorApp.exe" -ErrorAction SilentlyContinue) {
    $locked = $true
    $retry = 0
    while ($locked -and $retry -lt 5) {
        try {
            $f = [System.IO.File]::Open("bin\Debug\net8.0-windows\TranslatorApp.exe", 'Open', 'Read', 'None')
            $f.Close()
            $locked = $false
        } catch {
            $retry++
            Write-Host "  等待旧进程退出... ($retry/5)" -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        }
    }
    if ($locked) {
        Write-Host "[警告] 无法释放文件锁定，将继续尝试构建..." -ForegroundColor Yellow
    }
}

Remove-Item -Path $OutputDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

Write-Host "========================================" -ForegroundColor Cyan
if ($SelfContained) {
    Write-Host "  构建独立可执行文件 (无需 .NET 运行时)" -ForegroundColor Cyan
} else {
    Write-Host "  构建框架依赖版本 (需要 .NET 运行时)" -ForegroundColor Cyan
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($SelfContained) {
    Write-Host "[1/2] 发布独立 exe (Release, 单文件, 自包含)..." -ForegroundColor Green
    Write-Host "      目标: $OutputDir\TranslatorApp.exe" -ForegroundColor Gray
    Write-Host ""

    & $DotNetExe publish $ProjectFile `
        -c Release `
        --self-contained true `
        -r win-x64 `
        -o $OutputDir `
        /p:DebugType=None `
        /p:Optimize=true 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n[错误] 构建失败!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    # Remove PDB and other extra files
    Remove-Item -Path "$OutputDir\*.pdb" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$OutputDir\App.config" -Force -ErrorAction SilentlyContinue
} else {
    Write-Host "[1/2] 发布框架依赖版本 (Release, 单文件)..." -ForegroundColor Green
    Write-Host ""

    & $DotNetExe publish $ProjectFile `
        -c Release `
        -o $OutputDir `
        /p:DebugType=None 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n[错误] 构建失败!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    Remove-Item -Path "$OutputDir\*.pdb" -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "[2/2] 清理完成" -ForegroundColor Green
Write-Host ""

$exePath = Join-Path $OutputDir "TranslatorApp.exe"
if (Test-Path -LiteralPath $exePath) {
    $size = (Get-Item -LiteralPath $exePath).Length

    if ($size -gt 1GB) {
        $sizeStr = "{0:N2} GB" -f ($size / 1GB)
    } elseif ($size -gt 1MB) {
        $sizeStr = "{0:N1} MB" -f ($size / 1MB)
    } else {
        $sizeStr = "{0:N0} KB" -f ($size / 1KB)
    }

    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  构建完成!" -ForegroundColor Green
    Write-Host "  输出: $exePath" -ForegroundColor White
    Write-Host "  大小: $sizeStr" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "直接双击运行即可，程序会在系统托盘运行" -ForegroundColor Yellow
    Write-Host "默认快捷键: Alt + C" -ForegroundColor Yellow
} else {
    Write-Host "[错误] 未找到输出文件" -ForegroundColor Red
    exit 1
}
