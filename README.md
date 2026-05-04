# TcxEditor

A WPF desktop application for creating and editing Garmin TCX activity files (.tcx), built with C# and .NET 9.

## Features

- **Open / Save** — load existing `.tcx` files or create new ones from scratch
- **Activity** — set sport type (Running, Biking, Other) and start date/time
- **Laps** — add, edit and delete laps with full stats: duration, distance, speed, calories, heart rate
- **Trackpoints** — add, edit and delete GPS trackpoints with: coordinates, altitude, distance, heart rate, cadence, speed
- **Auto-time** — distribute timestamps across trackpoints proportionally to distance
- **Validation** — checks the document for errors and warnings before saving:
  - Errors block the save (e.g. negative speed, coordinates out of range, non-chronological timestamps)
  - Warnings require confirmation (e.g. heart rate out of range, decreasing distance)
- **Change date** — shift the date of the entire activity (all laps and trackpoints) while keeping the time of day
- **Shift time** — move all timestamps forward or backward by hours / minutes / seconds

## Requirements

- Windows 10 or later
- [.NET 9 Runtime (Windows)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## Build

```bash
dotnet build TcxEditor.sln
```

## Run

```bash
dotnet run --project TcxEditor.csproj
```

## TCX compatibility

Output files conform to the [Garmin TrainingCenterDatabase v2 schema](http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd) and can be imported into Garmin Connect, Strava, and other platforms that accept `.tcx` files.
