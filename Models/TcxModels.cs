using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TcxEditor.Models;

public class TcxDatabase
{
    public ObservableCollection<TcxActivity> Activities { get; set; } = new();
}

public class TcxActivity : INotifyPropertyChanged
{
    private string _sport = "Running";
    private DateTime _startTime = DateTime.Now;
    private string _notes = "";

    public string Sport
    {
        get => _sport;
        set { _sport = value; OnPropertyChanged(); }
    }

    public DateTime StartTime
    {
        get => _startTime;
        set { _startTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(StartTimeUtc)); }
    }

    public string StartTimeUtc => StartTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

    public string Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(); }
    }

    public ObservableCollection<TcxLap> Laps { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class TcxLap : INotifyPropertyChanged
{
    private DateTime _startTime = DateTime.Now;
    private double _totalTimeSeconds;
    private double _distanceMeters;
    private double _maximumSpeed;
    private int _calories;
    private int? _averageHeartRate;
    private int? _maximumHeartRate;
    private string _intensity = "Active";
    private string _notes = "";

    public DateTime StartTime
    {
        get => _startTime;
        set { _startTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(StartTimeUtc)); }
    }

    public string StartTimeUtc => StartTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

    public double TotalTimeSeconds
    {
        get => _totalTimeSeconds;
        set { _totalTimeSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(Duration)); }
    }

    public string Duration => TimeSpan.FromSeconds(TotalTimeSeconds).ToString(@"hh\:mm\:ss");

    public double DistanceMeters
    {
        get => _distanceMeters;
        set { _distanceMeters = value; OnPropertyChanged(); OnPropertyChanged(nameof(DistanceKm)); }
    }

    public double DistanceKm => Math.Round(DistanceMeters / 1000.0, 3);

    public double MaximumSpeed
    {
        get => _maximumSpeed;
        set { _maximumSpeed = value; OnPropertyChanged(); }
    }

    public int Calories
    {
        get => _calories;
        set { _calories = value; OnPropertyChanged(); }
    }

    public int? AverageHeartRate
    {
        get => _averageHeartRate;
        set { _averageHeartRate = value; OnPropertyChanged(); }
    }

    public int? MaximumHeartRate
    {
        get => _maximumHeartRate;
        set { _maximumHeartRate = value; OnPropertyChanged(); }
    }

    public string Intensity
    {
        get => _intensity;
        set { _intensity = value; OnPropertyChanged(); }
    }

    public string Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(); }
    }

    public ObservableCollection<TcxTrackpoint> Trackpoints { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class TcxTrackpoint : INotifyPropertyChanged
{
    private DateTime _time = DateTime.Now;
    private double? _latitudeDegrees;
    private double? _longitudeDegrees;
    private double? _altitudeMeters;
    private double? _distanceMeters;
    private int? _heartRateBpm;
    private int? _cadence;
    private double? _speed;

    public DateTime Time
    {
        get => _time;
        set { _time = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimeUtc)); }
    }

    public string TimeUtc => Time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

    public double? LatitudeDegrees
    {
        get => _latitudeDegrees;
        set { _latitudeDegrees = value; OnPropertyChanged(); }
    }

    public double? LongitudeDegrees
    {
        get => _longitudeDegrees;
        set { _longitudeDegrees = value; OnPropertyChanged(); }
    }

    public double? AltitudeMeters
    {
        get => _altitudeMeters;
        set { _altitudeMeters = value; OnPropertyChanged(); }
    }

    public double? DistanceMeters
    {
        get => _distanceMeters;
        set { _distanceMeters = value; OnPropertyChanged(); }
    }

    public int? HeartRateBpm
    {
        get => _heartRateBpm;
        set { _heartRateBpm = value; OnPropertyChanged(); }
    }

    public int? Cadence
    {
        get => _cadence;
        set { _cadence = value; OnPropertyChanged(); }
    }

    public double? Speed
    {
        get => _speed;
        set { _speed = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public static class SportTypes
{
    public static readonly string[] All = { "Running", "Biking", "Other" };
}

public static class IntensityTypes
{
    public static readonly string[] All = { "Active", "Resting" };
}
