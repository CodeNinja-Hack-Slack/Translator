@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo   Translator - 构建可执行文件
echo ========================================
echo.

set "DOTNET_DIR=E:\Software\NET8"
set "PATH=%DOTNET_DIR%;%PATH%"

where dotnet >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [错误] 未找到 .NET SDK
    pause
    exit /b 1
)

echo [1/2] 清理旧版本...
if exist "dist" rmdir /s /q dist
echo.

echo [2/2] 正在构建独立可执行文件...
echo       输出: dist\TranslatorApp.exe
echo.

dotnet publish "%~dp0TranslatorApp\TranslatorApp.csproj" ^
    -c Release ^
    --self-contained true ^
    -r win-x64 ^
    -o "%~dp0dist" ^
    /p:DebugType=None ^
    /p:Optimize=true

if %ERRORLEVEL% neq 0 (
    echo.
    echo [错误] 构建失败!
    pause
    exit /b %ERRORLEVEL%
)

if exist "%~dp0dist\*.pdb" del "%~dp0dist\*.pdb"
if exist "%~dp0dist\*.config" del "%~dp0dist\*.config"

echo.
echo ========================================
echo   构建完成!
echo   输出: %~dp0dist\TranslatorApp.exe
echo ========================================
echo.
echo 直接双击运行即可，程序会在系统托盘运行
echo 默认快捷键: Alt + C
echo.
pause
