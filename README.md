# Translator - 中英翻译桌面应用

轻量级中英互译桌面工具，后台运行，快捷键呼出翻译窗口。

## 功能

- **快捷键翻译** — 默认 `Alt + C` 呼出翻译窗口（可在设置中自定义）
- **多翻译源** — 默认使用 Bing 翻译（免费免配置），可切换百度翻译 / 有道翻译
- **自动语言检测** — 输入中文自动译为英文，输入英文自动译为中文
- **后台运行** — 启动后驻留系统托盘（蓝色"译"字图标）
- **开机自启动** — 安装时可勾选，或稍后通过设置启用

## 下载

[最新版本 Releases](https://github.com/CodeNinja-Hack-Slack/Translator/releases)

- 安装包：`Translator_Setup.exe` (~2.3 MB)
- 基于 .NET Framework 4.8（Windows 10 / 11 预装，无需额外安装运行时）

## 快速开始

1. 下载安装包并运行安装
2. 安装后程序自动在后台运行，托盘出现蓝色"译"字图标
3. 按 `Alt + C` 呼出翻译窗口，输入文字后按 Enter 或点击翻译
4. 右键托盘图标可打开设置 / 退出

## 构建

需要 .NET SDK 8.0+ 和 Inno Setup 6。

```powershell
# 编译应用
dotnet publish TranslatorApp\TranslatorApp.csproj -c Release -o build-fd\publish

# 编译安装包（需要 Inno Setup）
ISCC.exe installer.iss
```

## 技术栈

- **语言 / 框架**：C#，.NET Framework 4.8，WinForms
- **翻译 API**：Bing（Microsoft Translator）、百度翻译、有道翻译
- **快捷键**：Win32 `RegisterHotKey`
- **安装包**：Inno Setup 6
