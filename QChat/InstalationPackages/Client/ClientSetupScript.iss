; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "QChat CLient"
#define MyAppVersion "0.1"
#define MyAppPublisher "KeterSoftware"
#define MyAppExeName "QChat.CLient.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{1CCC6795-B4C7-40B0-B6A7-D72C130C660A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=D:\Gram\Dev\KS-Intern\QChat\InstalationPackages\Client
OutputBaseFilename=QChatCLientInstaller
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\Gram\Dev\KS-Intern\QChat\InstalationPackages\Client\Source\QChat.CLient.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Gram\Dev\KS-Intern\QChat\InstalationPackages\Client\Source\QChat.CLient.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Gram\Dev\KS-Intern\QChat\InstalationPackages\Client\Source\QChat.CLient.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Gram\Dev\KS-Intern\QChat\InstalationPackages\Client\Source\QChat.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

