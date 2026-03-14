# Bandit Voice Fix

Bandit Voice Fix is a Mount and Blade II: Bannerlord singleplayer mod that restores missing voiced dialogue for common outlaw encounters, including bandit parties, deserters, and hideouts.

The goal is simple: when you run into these conversations in campaign play, they should sound like proper voiced Bannerlord dialogue instead of dropping into silent lines.

## Why This Mod Exists

Based on TaleWorlds patch notes and local testing, the problem appears tied to the Bannerlord `1.3.x` update cycle rather than earlier `1.2.x` builds.

Two official patch-note points stand out:

- TaleWorlds' `v1.2.12` patch notes from December 2, 2024 do not mention the outlaw dialogue regressions this mod targets.
- TaleWorlds' `Beta v1.3.0` patch notes from September 15, 2025 introduced major conversation, hideout, and audio changes and explicitly listed: `Fixed an issue where Mountain Bandit dialogue appeared empty.`

That does not prove every missing outlaw voice line started on exactly `v1.3.0`, but it does strongly support the idea that the regression belongs to the `1.3.x` branch. In practice, this mod is intended for `1.3.x`-era Bannerlord builds and is generally not needed on `1.2.12` and earlier.

## Features

- Restores missing voiced dialogue for bandit-related encounters
- Covers bandit parties, deserters, and hideout conversations
- Includes optional debug logging through MCM
- Writes support-friendly logs only when debug logging is enabled
- Preserves the previous debug session log as `BanditVoiceFix.previous.log`

## Requirements

- Mount and Blade II: Bannerlord
- Harmony module for Bannerlord
- Optional for in-game debug settings: MCM / `Bannerlord.MBOptionScreen`

## Compatibility

- Primary target: Bannerlord `1.3.x`
- Generally not needed: Bannerlord `1.2.12` and earlier
- If TaleWorlds fully resolves the remaining outlaw voice path issues in a later patch, this mod may become unnecessary

## Installation

1. Download and install the mod into your Bannerlord `Modules` folder.
2. Make sure `BanditVoiceFix` is enabled in the launcher.
3. Keep Harmony and MCM installed if you want the full mod setup and settings support.

The module folder should look like this:

```text
Mount & Blade II Bannerlord
\- Modules
   \- BanditVoiceFix
      |- SubModule.xml
      \- bin
         \- Win64_Shipping_Client
            \- BanditVoiceFix.dll
```

## Debug Logging

If MCM is installed, the mod exposes an `Enable Debug Logging` setting.

When enabled, logs are written to:

```text
Modules\BanditVoiceFix\Logs\BanditVoiceFix.log
```

The previous session is rotated to:

```text
Modules\BanditVoiceFix\Logs\BanditVoiceFix.previous.log
```

No log folder or log files are created unless debug logging is turned on.

## Development

This repository contains the Visual Studio project used to build the mod:

- `BanditVoiceFix.sln`
- `BanditVoiceFix.csproj`

The project targets `.NET Framework 4.7.2` and references local Bannerlord assemblies from a standard Windows game install.

## Changelog

See [CHANGELOG.md](./CHANGELOG.md) for release history.

## Links

- Nexus Mods: <https://www.nexusmods.com/mountandblade2bannerlord/mods/10374>
- GitHub: <https://github.com/andor-ohm/BanditVoiceFix>
- TaleWorlds Patch Notes v1.2.12: <https://www.taleworlds.com/en/News/566>
- TaleWorlds Beta Patch Notes v1.3.0: <https://www.taleworlds.com/en/News/581>
