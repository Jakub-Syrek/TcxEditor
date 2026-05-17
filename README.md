# TcxEditor

A WPF desktop editor for Garmin TCX activity files.

![CI](https://github.com/Jakub-Syrek/TcxEditor/actions/workflows/tests.yml/badge.svg)
![Release](https://img.shields.io/github/v/release/Jakub-Syrek/TcxEditor)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![License](https://img.shields.io/github/license/Jakub-Syrek/TcxEditor)
![Last commit](https://img.shields.io/github/last-commit/Jakub-Syrek/TcxEditor)

## Overview

TcxEditor is a native Windows desktop application for inspecting, repairing
and authoring Garmin TrainingCenterDatabase v2 (`.tcx`) files. It targets
athletes and engineers who need precise, schema-correct edits to GPS tracks,
laps, heart-rate streams and timestamps before re-uploading the activity to
Garmin Connect, Strava or any TCX-aware platform.

The application is written in C# on .NET 9 with WPF for the UI. Domain logic
is intentionally isolated from the UI layer so it can be unit-tested and
re-hosted in other front-ends in the future.

## Features

- Open and save `.tcx` files conforming to Garmin's v2 schema.
- Set activity metadata: sport (Running / Biking / Other), start time, notes.
- Add, edit and delete laps with full statistics: duration, distance,
  maximum speed, calories, average and maximum heart rate, intensity.
- Add, edit and delete GPS trackpoints with coordinates, altitude, distance,
  heart rate, cadence and speed.
- Auto-time distribution: spread timestamps across trackpoints
  proportionally to distance.
- Date shifting: move the entire activity to a new date while preserving the
  time-of-day.
- Time shifting: move every timestamp forward or backward by an arbitrary
  offset.
- Pre-save validation with a clear error/warning split:
  - Errors block save (negative speed, coordinates out of range,
    non-chronological timestamps).
  - Warnings require explicit confirmation (heart rate out of range,
    decreasing distance).
- Full undo / redo backed by the Command + Memento patterns.

## Architecture

The codebase is organised around classic design patterns to keep the WPF
shell thin and the domain testable:

| Layer | Folder | Responsibility |
|-------|--------|----------------|
| Views | `Views/` | WPF dialogs (lap editor, trackpoint editor, change date, shift time). |
| ViewModels | `ViewModels/` | Presentation glue between views and the model. |
| Models | `Models/` | `TcxDatabase`, `TcxActivity`, `TcxLap`, `TcxTrackpoint`, `ValidationIssue`. |
| Services | `Services/` | `TcxService` (XML load/save facade), `ValidationService`. |
| Repositories | `Repositories/` | `ITcxRepository` + file-system implementation. |
| Builders | `Builders/` | Fluent `TcxLapBuilder`, `TcxTrackpointBuilder`. |
| Factories | `Factories/` | `TcxDialogFactory` for dialog instantiation. |
| Commands | `Commands/` | `IUndoableCommand` implementations + `CommandHistory` invoker. |
| Validation | `Validation/` | Strategy pattern: activity / lap / trackpoint validators. |

## Screenshots

_Coming soon._

## Build and run

Requirements:

- Windows 10 or later.
- [.NET 9 SDK (Windows)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

```powershell
dotnet build TcxEditor.sln -c Release
dotnet run --project TcxEditor.csproj
```

## Testing

Tests live in `TcxEditor.Tests` and use **NUnit** with **NSubstitute** for
mocking. They target the pure domain logic (parsing, builders, command
history, validation strategies).

```powershell
dotnet test TcxEditor.sln -c Release
```

CI runs the same command on every push and pull request to `master`.

## Versioning

Releases follow [Semantic Versioning](https://semver.org/). The version is
derived automatically from
[Conventional Commits](https://www.conventionalcommits.org/) on `master`:

- `feat:` -> minor bump.
- `fix:`, `docs:`, `test:`, `refactor:`, `perf:`, `chore:`, `ci:` -> patch.
- Any commit body containing `BREAKING CHANGE:` -> major.

The `.github/workflows/version.yml` workflow performs the bump, updates
`<Version>` in `TcxEditor.csproj`, tags `vX.Y.Z` and publishes a GitHub
Release with auto-generated notes.

## License

Distributed under the MIT License. See [LICENSE](LICENSE) for details.

## Contact

Jakub Syrek &lt;jakubvonsyrek@gmail.com&gt;
