# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-05-17

### Added

- Initial public release of TcxEditor as a WPF desktop application
  targeting .NET 9 on Windows.
- TCX file I/O: `TcxService` reads and writes Garmin
  TrainingCenterDatabase v2 documents (`Services/TcxService.cs`).
- Repository abstraction over the file system
  (`Repositories/ITcxRepository.cs`, `Repositories/TcxFileRepository.cs`).
- Domain model: `TcxDatabase`, `TcxActivity`, `TcxLap`, `TcxTrackpoint`,
  `ValidationIssue` (`Models/`).
- Fluent builders for laps and trackpoints
  (`Builders/TcxLapBuilder.cs`, `Builders/TcxTrackpointBuilder.cs`).
- Dialog factory for view instantiation
  (`Factories/TcxDialogFactory.cs`).
- Undoable Command + Memento implementations for add / edit / delete
  laps and trackpoints, auto-time distribution, time shifting, and a
  history invoker with redo support (`Commands/`).
- Strategy-pattern validators for activities, laps and trackpoints with a
  facade entry point in `ValidationService` (`Validation/`, `Services/`).
- WPF dialogs for editing laps, editing trackpoints, changing the activity
  date and shifting timestamps (`Views/`).
- English-only UI (translated from the original Polish prototype).

[Unreleased]: https://github.com/Jakub-Syrek/TcxEditor/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/Jakub-Syrek/TcxEditor/releases/tag/v1.0.0
