# About TcxEditor

TcxEditor is a Windows desktop editor for Garmin TCX activity files. It
opens, repairs and authors `.tcx` documents that conform to Garmin's
TrainingCenterDatabase v2 schema, so the resulting files import cleanly into
Garmin Connect, Strava and any other TCX-aware platform.

The application is built in C# on .NET 9 with a WPF front-end. Domain logic
is kept strictly separate from the UI: parsing and serialisation, validation
strategies, undo/redo commands and value builders all live in pure classes
that can be unit-tested and re-hosted. The codebase deliberately leans on
classic design patterns (Repository, Strategy, Builder, Factory, Command,
Memento, Facade) to keep responsibilities small and changes safe.

Typical use cases include fixing corrupted timestamps, shifting an activity
to a different day, redistributing trackpoint times proportionally to
distance, and stripping or normalising heart-rate and cadence streams
before re-uploading.
