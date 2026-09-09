; Inno Setup script for Linker — packages the .NET Framework 4.8 publish output (see
; ../LinkerWpf/publish/, produced by: dotnet publish -c Release -o publish) into a per-user
; installer. Per-user (no admin elevation) matches the app's own design — every registration it
; does (default-browser candidacy, settings) is already HKCU/per-user only, so the installer
; shouldn't need admin rights either. Saved data is a flat JSON file (see Data/JsonDataStore.cs),
; not a database — no native DB driver to ship.
;
; .NET Framework 4.8 is pre-installed on Windows 10 1903+ and Windows 11, so the publish output is
; just the application assembly (Linker.exe) plus Newtonsoft.Json.dll — [Files] below packages the
; whole publish folder rather than a single exe, excluding the .pdb (debug symbols, not needed for
; distribution).

#define AppName "Linker"
; Overridable from the command line (/DAppVersion=1.2.3) so the manually-triggered CI release
; workflow (.github/workflows/release.yml) can stamp the installer with whatever version was typed
; into (or auto-bumped for) that run — local manual builds with no override still default sensibly.
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define AppPublisher "Linker"
#define AppExeName "Linker.exe"
#define AppId "{{A6E0F5B2-6C2E-4C1E-9C1D-8B5B8C6E9A21}"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=Output
OutputBaseFilename=LinkerSetup
SetupIconFile=..\LinkerWpf\Assets\app.ico
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "..\LinkerWpf\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName} now"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Registry]
; Cleans up the per-user registry entries DefaultBrowserRegistration.EnsureRegistered() writes on
; every app startup (Util/DefaultBrowserRegistration.cs) — same keys, mirrored here so uninstalling
; doesn't leave Linker listed as a browser candidate in Windows Settings after it's gone. All HKCU,
; matching the app's own no-admin-required registration.
Root: HKCU; Subkey: "Software\Clients\StartMenuInternet\Linker"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Linker.Url"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueType: none; ValueName: "Linker"; Flags: deletevalue uninsdeletevalue

[Code]
// Nothing custom needed yet — EnsureRegistered() runs itself on first launch
// (App.xaml.cs OnStartup), so the installer doesn't need to write the
// browser-candidacy keys itself.
