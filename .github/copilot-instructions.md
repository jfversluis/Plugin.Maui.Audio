# Plugin.Maui.Audio - Copilot Instructions

## Project Overview

This is a .NET MAUI plugin that provides cross-platform audio playback and recording. It targets Android, iOS, macOS (Catalyst), and Windows.

## Architecture

Two main features: **AudioPlayer** (play from files/streams/URLs) and **AudioRecorder** (record from mic).

Key interfaces: `IAudioPlayer`, `IAudio`, `IAudioRecorder`, `IAudioSource`, `AsyncAudioPlayer`.

Platform implementations:
- Android: `MediaPlayer`
- iOS/macOS: `AVAudioPlayer`
- Windows: `MediaPlayer`

### Platform Notes
- Speed ranges differ: Android 0-2.5, iOS 0.5-2, Windows 0-8
- Balance: -1 (left) to 1 (right)
- Options classes are platform-specific (`AudioPlayerOptions`, `AudioRecorderOptions`)

## Code Conventions

### Namespace
Root namespace prefix is `Plugin.Maui.Audio` (with sub-namespaces like `Plugin.Maui.Audio.AudioListeners`).

### File Naming
- `*.shared.cs` - Cross-platform code
- `*.android.cs` - Android-specific code
- `*.macios.cs` - iOS/macOS-specific code
- `*.windows.cs` - Windows-specific code
- `*.ios.cs` - iOS-only code
- `*.net.cs` - Generic .NET fallback

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
