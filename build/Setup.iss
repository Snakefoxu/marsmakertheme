#define MyAppName "SnakeMarsTheme"
#define MyAppVersion "1.0.7"
#define MyAppPublisher "SnakeFoxu"
#define MyAppExeName "SnakeMarsTheme.exe"
#define OutputPath "..\releases"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8B456721-C123-4567-89AB-CDEFG1234567}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Require admin privileges for Program Files installation
PrivilegesRequired=admin
OutputDir={#OutputPath}
OutputBaseFilename=SnakeMarsTheme_Setup_v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

; Imágenes y Branding
WizardImageFile=..\assets\branding\installer_banner.bmp
SetupIconFile=..\assets\branding\app_icon.ico

; Arquitectura x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Fuente files extraídos temporalmente por Build-Installer.ps1
Source: "temp_installer\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  LocalAppData: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Preguntar por datos de usuario en Documentos
    if MsgBox('¿Desea eliminar también todos los temas descargados y configuraciones de usuario?' #13#13 'Esto borrará permanentemente la carpeta: ' + ExpandConstant('{userdocs}\SnakeMarsTheme'), mbConfirmation, MB_YESNO) = IDYES then
    begin
      DelTree(ExpandConstant('{userdocs}\SnakeMarsTheme'), True, True, True);
    end;
    
    // Limpiar datos legacy en LocalAppData (FFmpeg descargado, etc.)
    LocalAppData := ExpandConstant('{localappdata}\SnakeMarsTheme');
    if DirExists(LocalAppData) then
    begin
      DelTree(LocalAppData, True, True, True);
    end;
  end;
end;
