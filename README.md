<p align="center">
  <img src="LinkerWpf/Assets/icon-256.png" width="96" height="96" alt="Linker icon">
</p>

<h1 align="center">Linker</h1>

<p align="center">
  Pick which browser opens a link — every time.
</p>

<p align="center">
  <a href="https://github.com/jehan593/linker-windows/releases/latest"><img src="https://img.shields.io/github/v/release/jehan593/linker-windows?label=release" alt="Latest release"></a>
  <img src="https://img.shields.io/badge/.NET%20Framework-4.8-512BD4" alt=".NET Framework 4.8">
  <img src="https://img.shields.io/badge/platform-Windows-0078D6" alt="Windows">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green" alt="MIT License"></a>
</p>

---

> **FYI:** this project is fully vibe-coded

Windows only lets you set one default browser. Linker sits in front of that:
open any link, a small popup asks which browser should handle it. Built with
WPF on .NET Framework 4.8, which ships with Windows — no runtime to install.

## Features

- Choose a browser every time you open a link
- Add, remove, and reorder your browsers
- Save links to open later, with search
- Light/dark theme that follows Windows
- No admin rights required

## Install

Download `LinkerSetup.exe` from the [Releases page](https://github.com/jehan593/linker-windows/releases/latest)
and run it. It installs per-user, no admin prompt.

Then set Linker as your default app for `HTTP` and `HTTPS`:
Windows Settings → Apps → Default apps.

## Build from source

Needs the [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48).

```sh
git clone https://github.com/jehan593/linker-windows.git
cd linker-windows
dotnet run --project LinkerWpf
```

.NET Framework 4.8 is pre-installed on Windows 10 1903+ and Windows 11, so the
built app runs without any runtime install. For a per-user installer, see
`installer/linker.iss`.

## License

[MIT](LICENSE)