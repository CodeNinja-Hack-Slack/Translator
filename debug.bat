@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo   Translator - 中英翻译工具
echo ========================================
echo.

set "DOTNET_DIR=E:\Software\NET8"
set "PATH=%DOTNET_DIR%;%PATH%"

where dotnet >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [错误] 未找到 .NET SDK: %DOTNET_DIR%
    echo 请安装 .NET 8 SDK 或修改 debug.bat 中的 DOTNET_DIR 路径
    pause
    exit /b 1
)

echo [1/2] 正在构建项目...
dotnet build "%~dp0TranslatorApp\TranslatorApp.csproj" -c Debug
if %ERRORLEVEL% neq 0 (
    echo.
    echo [错误] 构建失败!
    pause
    exit /b %ERRORLEVEL%
)
echo       构建成功!
echo.

echo [2/2] 正在启动程序...
echo       托盘图标出现后即可使用
echo       快捷键: Alt+C      = 划词翻译
echo       Alt+Shift+C = 输入翻译
echo       右键托盘图标可打开设置
echo       关闭此窗口即终止程序
echo.

dotnet run --project "%~dp0TranslatorApp\TranslatorApp.csproj" -c Debug

echo.
echo [信息] 程序已退出
pause
