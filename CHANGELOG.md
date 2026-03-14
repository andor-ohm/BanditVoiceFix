# Changelog

## 1.0.1 - 2026-03-13

- Added session-based log rotation so the current support log stays small and the previous session is preserved as `BanditVoiceFix.previous.log`.
- Added clear conversation begin/end markers with party identifiers to make support logs easier to read.
- Simplified debug logging into a support-focused log that records only important events.
- Added richer missing-voice details, including speaker, accent, line text, and available voice token summaries to help expand fallback coverage.
- Changed logging so no `Logs` folder or log file is created unless debug logging is enabled.

## 1.0.0 - 2026-03-12

- Initial release
- Restored missing voiced dialogue for bandit parties, deserters, and hideouts
- Added optional MCM debug logging