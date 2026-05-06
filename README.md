# YTP++ Deluxe

YTP++ Deluxe is a WinForms desktop application that builds chaotic YouTube Poop-style edits from imported video and audio libraries, applies randomized modular effects, and renders a final MP4 through FFmpeg.

## Features

- Folder-based media libraries for Materials, Transitions, Intros, Outros, Overlays, Sound FX, Music, Rave, and Vocoder assets.
- Supports MP4 and MP3 by default, with AVI, MOV, MKV, WMV, WAV, AAC, M4A, and OGG included in the importer.
- Random clip selection, slicing, recombination, chaos timeline shuffle, and auto source switching.
- Modular effect system in `EffectsFactory.cs` with video, audio, hybrid, special, and post-render effects.
- Effect stacking with duplicate prevention, per-effect enable state, probability, and intensity.
- Preset save/load support, including the default "YTP Deluxe Complete" preset.
- External FFmpeg rendering with a temporary segment pipeline and final MP4 output.

## Build Configurations

- `YTP++ Legacy`: targets .NET Framework 4.0 for Windows XP, Vista, 7, 8, and 8.1-era systems.
- `YTP++ Modern`: targets .NET Framework 4.8 for Windows 10 and Windows 11.

## Requirements

- Visual Studio 2013 or newer for the legacy configuration.
- Visual Studio 2022 for the modern configuration.
- FFmpeg installed separately. The app can use either `ffmpeg.exe` on `PATH` or a configured executable path.
- No WPF and no unsafe code are used.

## Build Instructions

1. Open `YTP++.sln` in Visual Studio.
2. Choose either `YTP++ Legacy` or `YTP++ Modern` from the solution configuration dropdown.
3. Build the solution.
4. Run `YTP++ Deluxe.exe` from the selected `bin` output folder.

## FFmpeg Setup

1. Download a Windows FFmpeg build.
2. Extract it somewhere stable, such as `C:\Tools\ffmpeg`.
3. Either add the `bin` folder to `PATH`, or open the Render Settings tab and browse directly to `ffmpeg.exe`.
4. The default `App.config` value is `ffmpeg.exe`, which works when FFmpeg is on `PATH` or beside the application executable.

## Importing Media

1. Open the Media Libraries tab.
2. Import at least one folder into Materials. This library is required.
3. Optionally import Transitions, Intros, Outros, Overlays, Sound FX, Music, Rave, and Vocoder folders.
4. The importer scans folders recursively.

## Rendering

1. Open the Effects Panel tab and enable the effects you want.
2. Select an effect to adjust its probability and intensity.
3. Adjust stack level to control how many unique effects can be applied to a clip.
4. Open Render Settings, choose the FFmpeg path and output MP4 path, then click Start Render.
5. Progress and FFmpeg output are shown in the log console.

## Performance Notes

- Rendering is CPU-heavy because every segment is transcoded before concatenation.
- Lower clip count and stack level for old machines.
- Some FFmpeg builds may not include every optional audio filter. If a render fails, disable the effect mentioned near the end of the log.
- Keep temporary cleanup enabled unless you need to inspect intermediate files.

## Zip-Ready Structure

The repository is already arranged as a zip-ready Visual Studio solution:

```text
YTP++/
  YTP++.sln
  YTP++/
    Program.cs
    Main.cs
    Main.Designer.cs
    YTPGenerator.cs
    EffectsFactory.cs
    Utilities.cs
    AboutBox.cs
    AboutBox.Designer.cs
    AboutBox.resx
    App.config
    packages.config
    iconwide.ico
    Properties/
  README.md
  LICENSE
  .gitignore
```
