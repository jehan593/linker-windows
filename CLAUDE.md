# CLAUDE.md

Guidance for Claude Code when working with code in this repository.

## What this is

Linker for Windows: a WPF (.NET Framework 4.8) desktop app that lets you pick
which browser opens a link. It registers itself as a Windows browser and shows
its own chooser popup instead of opening a link in a browser directly.

Core flow: opening any http/https link anywhere on the system runs `Linker.exe`
with the link as a command-line argument (Linker is registered as the URL
handler) → `App.xaml.cs` sees the link and shows `LinkChooserWindow` instead of
the normal main window → the chosen browser opens the link.

## Commands

```sh
dotnet build                  # from LinkerWpf/ or repo root
dotnet run --project LinkerWpf  # launches the main window (no link arg)
dotnet run --project LinkerWpf -- https://example.com  # launches the link chooser popup
```

No test suite exists.

Requires the .NET Framework 4.8 Developer Pack to build. .NET Framework 4.8 is
pre-installed on Windows 10 1903+ and Windows 11, so end-users need no runtime.

## How it's structured

- Every launch is a fresh process. `App.xaml.cs OnStartup` decides: a link
  argument shows `Interceptor/LinkChooserWindow`, no argument shows
  `MainWindow`. Both build their own `Di/AppContainer` and use the same data
  files — there is no cross-process IPC.
- **`MainWindow`** — two-tab launcher (`Ui/Browsers/ManageBrowsersView`,
  `Ui/SavedLinks/SavedLinksView`) with a `Ui/Home/DefaultBrowserBanner`.
  Default-browser status is rechecked on `Window.Activated`. Data is reloaded
  there too, since a separate `LinkChooserWindow` process can change the shared
  data file.
- **`LinkChooserWindow`** — small, `Topmost`, `ShowInTaskbar="False"` popup,
  centered on screen. Picking a browser (or Cancel) calls `Close()`, which ends
  the process.
- **Data** — no database. `Data/JsonDataStore.cs` persists browser prefs and
  saved links to one JSON file (`%AppData%\Linker\linker-data.json`), guarded
  by a named Mutex so two Linker processes can't corrupt it. Repositories are
  pull-based; ViewModels reload after their own mutations.
- **Browser pick-up** — `Data/Browser/InstalledBrowsersRepository.cs` scans
  the `Clients\StartMenuInternet` registry (HKLM + HKCU, plus WOW6432Node),
  the same place Windows Settings reads from. `Util/BrowserLauncher.cs` starts
  the picked browser's `.exe` directly with the link as an argument.
- **Default browser** — `Util/DefaultBrowserRegistration.cs` registers Linker
  per-user in `HKEY_CURRENT_USER` (no admin), and `IsDefault()` reads the
  `UserChoice` registry value Windows writes after the user picks Linker.
- **Theme** — `Theme/ColorsDark.xaml`/`ColorsLight.xaml` are swapped at runtime
  by `Theme/ThemeManager.cs`, following Windows' light/dark app setting.
- **Toasts** — `Ui/Components/ToastService.cs` + `ToastHostControl`, standing
  in for Android's toast.
- **JSON** — `Newtonsoft.Json` (WPF on .NET Framework 4.8 can't use
  `System.Text.Json` source generators).

## Style

Keep it simple: short, plain comments and UI text — clear over clever. This
also applies to this file, the README, and any user-facing copy.

## Building the installer

```sh
# 1. Publish (from LinkerWpf/)
dotnet publish -c Release -o publish

# 2. Compile the installer (from installer/)
"$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" linker.iss
# -> installer/Output/LinkerSetup.exe
```

The `[Registry]` section in `installer/linker.iss` mirrors
`DefaultBrowserRegistration`, so uninstalling removes Linker from Windows'
browser list. Keep the two in sync if that logic changes. Saved links/settings
under `%AppData%\Linker` are left alone on uninstall.

## Icon

`Assets/app.ico` is generated (not hand-drawn) by `Assets/generate-icon.ps1`.
Re-run that script if the glyph or palette ever changes — don't hand-edit
`app.ico`.

## Fonts

Martian Mono ships as three `.ttf`s under `Fonts/` (license copied alongside).
WPF resolves embedded fonts by their internal family name: regular/bold embed
as **"MartianMono NF"**, medium as **"MartianMono NF Med"**. `Theme/Fonts.xaml`
exposes both as `MartianMono`/`MartianMonoMedium`.