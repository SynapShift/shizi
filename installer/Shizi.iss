#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif

#ifndef PublishDir
  #define PublishDir "..\artifacts\release\publish"
#endif

#ifndef OutputDir
  #define OutputDir "..\artifacts\release"
#endif

#ifndef ChineseMessages
  #define ChineseMessages "..\artifacts\release\ChineseSimplified.isl"
#endif

[Setup]
AppId={{C3E4AF3B-4C2B-4D48-B6E4-CE7D85514004}
AppName=拾字 Shizi
AppVersion={#AppVersion}
AppPublisher=SynapShift
AppPublisherURL=https://github.com/SynapShift/shizi
AppSupportURL=https://github.com/SynapShift/shizi/issues
AppUpdatesURL=https://github.com/SynapShift/shizi/releases
DefaultDirName={localappdata}\Programs\Shizi
DefaultGroupName=拾字 Shizi
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#OutputDir}
OutputBaseFilename=Shizi-Setup-v{#AppVersion}-win-x64
SetupIconFile=..\src\Shizi\Assets\Shizi.ico
UninstallDisplayIcon={app}\Shizi.exe
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "chinesesimp"; MessagesFile: "{#ChineseMessages}"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加快捷方式："; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\拾字 Shizi"; Filename: "{app}\Shizi.exe"
Name: "{autodesktop}\拾字 Shizi"; Filename: "{app}\Shizi.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Shizi.exe"; Description: "启动拾字 Shizi"; Flags: nowait postinstall skipifsilent
