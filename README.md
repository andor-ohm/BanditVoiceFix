# Bandit Voice Fix

Bandit Voice Fix is a Mount & Blade II: Bannerlord singleplayer mod that restores missing voiced dialogue for bandit parties, deserters, and hideout encounters.

## Why This Mod Exists

Based on TaleWorlds patch notes and local testing, the problem appears tied to the Bannerlord `1.3.x` update cycle rather than earlier `1.2.x` builds.

Two official patch-note points stand out:

- TaleWorlds' `v1.2.12` patch notes from December 2, 2024 do not mention the outlaw dialogue regressions this mod targets.
- TaleWorlds' `Beta v1.3.0` patch notes from September 15, 2025 introduced major conversation, hideout, and audio changes and explicitly listed: `Fixed an issue where Mountain Bandit dialogue appeared empty.`

That does not prove every missing outlaw voice line started on exactly `v1.3.0`, but it does strongly support the idea that the regression belongs to the `1.3.x` branch. In practice, this mod is intended for `1.3.x`-era Bannerlord builds and is generally not needed on `1.2.12` and earlier.

## Features

- Restores missing voiced dialogue for bandit-related encounters
- Covers bandit parties, deserters, and hideout conversations
- Uses another bandit voice for the same dialogue when the preferred voice is unavailable
- Includes optional debug logging through a settings file; errors log automatically
- Preserves the previous debug session log as `BanditVoiceFix.previous.log`

## Requirements

- Mount & Blade II: Bannerlord
- Harmony module for Bannerlord

Harmony is the only third-party dependency.

## Compatibility

- Works with Bannerlord `1.3.x` through `1.4.8`
- Not needed on Bannerlord `1.2.12` and earlier
- If TaleWorlds fully resolves the outlaw voice issues, this mod may become unnecessary

## Installation

1. Download and install the mod into your Bannerlord `Modules` folder.
2. Make sure `BanditVoiceFix` is enabled in the launcher.
3. Load BanditVoiceFix after Harmony and the required game modules.

The module folder should look like this:

```text
Mount & Blade II Bannerlord
\- Modules
   \- BanditVoiceFix
      |- SubModule.xml
      |- BanditVoiceFix.settings.xml
      |- Logs
      |  |- BanditVoiceFix.log
      |  \- BanditVoiceFix.previous.log
      \- bin
         \- Win64_Shipping_Client
            \- BanditVoiceFix.dll
```

Log files are created only when an error occurs or debug logging is enabled. When logging starts in a later session, the existing log becomes `BanditVoiceFix.previous.log`.

## Debug Logging

Debug logging is off by default. Set `EnableDebugLogging` to `true` in `Modules\BanditVoiceFix\BanditVoiceFix.settings.xml`, then restart the game.

Errors and enabled debug messages are written to:

```text
Modules\BanditVoiceFix\Logs\BanditVoiceFix.log
```

The previous session is rotated to:

```text
Modules\BanditVoiceFix\Logs\BanditVoiceFix.previous.log
```

Log files are created only when a message is logged and rotate automatically between sessions.

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
