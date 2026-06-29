; Inno Setup 安装脚本
; 使用 Inno Setup 6 编译

#define MyAppName "Translator"
#define MyAppVersion "1.0"
#define MyAppPublisher "Translator"
#define MyAppURL "https://github.com/translator"
#define MyAppExeName "TranslatorApp.exe"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
OutputDir=installer
OutputBaseFilename=Translator_Setup
PrivilegesRequired=admin
SetupIconFile=TranslatorApp\app.ico
DisableProgramGroupPage=no
DisableReadyPage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "快捷方式:"
Name: "startup"; Description: "开机自动启动"; GroupDescription: "启动选项:"; Flags: checkedonce

[Files]
Source: "build-fd\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "运行 Translator"; Flags: nowait postinstall skipifsilent

[Registry]
; 开机自启动
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Translator"; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletevalue; Tasks: startup
