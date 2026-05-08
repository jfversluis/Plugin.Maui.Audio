# $ Copilot InstructionsREPO 

## Project Overview

This is a .NET MAUI plugin that provides cross-platform audio playback and recording. It targets Android, iOS, macOS (Catalyst), Windows.

### Architecture

Two main features: **AudioPlayer** (play from files/streams/URLs) and **AudioRecorder** (record from mic).

Key interfaces: `IAudioPlayer`, `IAudio`, `IAudioRecorder`, `IAudioSource`, `AsyncAudioPlayer`.

Platform implementations: MediaPlayer (Android), AVAudioPlayer (iOS), MediaPlayer (Windows).

### Platform Notes
- Speed ranges differ: Android 0-2.5, iOS 0.5-2, Windows 0-8
- Balance: -1 (left) to 1 (right)
- Options classes are platform-specific (AudioPlayerOptions, AudioRecorderOptions)

## Code Conventions

### Namespace
All code uses: `Plugin.Maui.Audio`

### File Naming
- `*.shared. Cross-platform codecs` 
- `*.android. Androidcs` 
- `*.macios. iOS/macOScs` 
- `*.windows. Windowscs` 
- `*.ios. iOS-onlycs` 
- `*.net. Generic .NET fallbackcs` 

### Standards
- File-scoped namespaces
- `camelCase` for private fields, `PascalCase` for public
- XML docs required on all public APIs
- Use `<inheritdoc/>` on implementations
- Null-conditional operators for platform interop objects

## Building

```bash
dotnet build src/Plugin.Maui.Audio/Plugin.Maui.Audio.csproj -c Release
```

## When Making Changes
1. Ensure the plugin builds on all target platforms
2. If adding public API, update the interface
3. Implement on all supported platforms
4. Add stub in `.net.cs` if applicable
5. Update sample app and README
