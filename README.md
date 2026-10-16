# Word-flash

A Windows tray app that flashes King James Version verses on top of whatever you are working on, at random intervals.

- Categories: Words of Jesus (Gospels), Proverbs, Psalms, Torah (Genesis–Deuteronomy), Prophets (Isaiah–Malachi), Paul's Epistles (Romans–Hebrews).
- Each verse appears at a random spot (top, bottom, left or right edge, or the centre) and stays long enough to read it: 2 s to notice it, plus its word count at your reading speed (Slow 150, Normal 180, Fast 220 words per minute), plus a random 0–20% extra, kept between 4 and 60 s. The next one follows after a random gap.
- The overlay is click-through and never takes focus. Text is outlined on a translucent backdrop, so it stays readable over light and dark windows.
- The tray menu has Flashing (on/off), Flash now, Settings… and Exit. Settings cover the categories, reading speed, gap between verses, font, size and Start with Windows.

Settings are stored in `%APPDATA%\WordFlash\settings.json`. Start with Windows uses `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\WordFlash`.

## Install

1. Download `WordFlash.App.exe` from the [latest release](https://github.com/cheddarking/word-flash/releases/latest). It is self-contained, so no .NET install is needed.
2. Put it in `%LOCALAPPDATA%\Programs\WordFlash` and run it. A tray icon appears.
3. Right-click the tray icon, open Settings… and tick Start with Windows.

Windows SmartScreen may warn you the first time, because the exe is not code-signed. Choose More info, then Run anyway.

To uninstall, untick Start with Windows, choose Exit, then delete the folder and `%APPDATA%\WordFlash`.

## Build and test

Requires the .NET 10 SDK on Windows. From WSL, call `dotnet.exe`.

```sh
dotnet.exe build -warnaserror
dotnet.exe test
dotnet.exe format --verify-no-changes
```

## Publish

```sh
dotnet.exe publish src/WordFlash.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
```

Copy `publish/WordFlash.App.exe` to a folder on a Windows drive, such as `%LOCALAPPDATA%\Programs\WordFlash`, and run it from there. If you tick Start with Windows while the app runs from a `\\wsl.localhost` path, sign-in will fail to start it whenever WSL isn't running.

## Verse data

`src/WordFlash.Core/Data/verses.json` is generated from the public-domain eBible.org KJV. The source, hash and the regeneration command are in [data/SOURCE.md](data/SOURCE.md). The filter rules (at least 6 words, at most 280 characters, ends with `.`, `?` or `!`, no genealogies) live in `tools/WordFlash.DataGen/VerseSelector.cs`.

## Layout

| Project | Purpose |
|---|---|
| `src/WordFlash.Core` | Verse catalog, settings, scheduler, layout maths (no WPF) |
| `src/WordFlash.App` | WPF tray app, overlay window, settings window, startup registration |
| `tools/WordFlash.DataGen` | Converts the USFM archive into `verses.json` |
| `tests/WordFlash.Tests` | xUnit tests for Core and DataGen |

## Licence

The code is MIT-licensed; see [LICENSE](LICENSE). The King James Version text is in the public domain.
