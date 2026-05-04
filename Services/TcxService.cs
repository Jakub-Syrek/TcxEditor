using System.Globalization;
using System.IO;
using System.Xml.Linq;
using TcxEditor.Models;

namespace TcxEditor.Services;

public static class TcxService
{
    private static readonly XNamespace Ns = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";
    private static readonly XNamespace NsExt = "http://www.garmin.com/xmlschemas/ActivityExtension/v2";
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static TcxDatabase Load(string path)
    {
        var doc = XDocument.Load(path);
        var root = doc.Root ?? throw new InvalidDataException("Brak głównego elementu XML.");
        var db = new TcxDatabase();

        foreach (var actEl in root.Descendants(Ns + "Activity"))
            db.Activities.Add(ParseActivity(actEl));

        return db;
    }

    public static void Save(TcxDatabase db, string path)
    {
        var doc = BuildDocument(db);
        doc.Save(path);
    }

    private static TcxActivity ParseActivity(XElement el)
    {
        var act = new TcxActivity
        {
            Sport = (string?)el.Attribute("Sport") ?? "Running",
            Notes = (string?)el.Element(Ns + "Notes") ?? ""
        };

        var idText = (string?)el.Element(Ns + "Id");
        if (DateTime.TryParse(idText, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            act.StartTime = dt.ToLocalTime();

        foreach (var lapEl in el.Elements(Ns + "Lap"))
            act.Laps.Add(ParseLap(lapEl));

        return act;
    }

    private static TcxLap ParseLap(XElement el)
    {
        var lap = new TcxLap
        {
            TotalTimeSeconds = ParseDouble(el.Element(Ns + "TotalTimeSeconds")),
            DistanceMeters = ParseDouble(el.Element(Ns + "DistanceMeters")),
            MaximumSpeed = ParseDouble(el.Element(Ns + "MaximumSpeed")),
            Calories = ParseInt(el.Element(Ns + "Calories")),
            AverageHeartRate = ParseHeartRate(el.Element(Ns + "AverageHeartRateBpm")),
            MaximumHeartRate = ParseHeartRate(el.Element(Ns + "MaximumHeartRateBpm")),
            Intensity = (string?)el.Element(Ns + "Intensity") ?? "Active",
            Notes = (string?)el.Element(Ns + "Notes") ?? ""
        };

        var startAttr = (string?)el.Attribute("StartTime");
        if (DateTime.TryParse(startAttr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            lap.StartTime = dt.ToLocalTime();

        var track = el.Element(Ns + "Track");
        if (track != null)
            foreach (var tpEl in track.Elements(Ns + "Trackpoint"))
                lap.Trackpoints.Add(ParseTrackpoint(tpEl));

        return lap;
    }

    private static TcxTrackpoint ParseTrackpoint(XElement el)
    {
        var tp = new TcxTrackpoint
        {
            AltitudeMeters = ParseNullableDouble(el.Element(Ns + "AltitudeMeters")),
            DistanceMeters = ParseNullableDouble(el.Element(Ns + "DistanceMeters")),
            HeartRateBpm = ParseHeartRate(el.Element(Ns + "HeartRateBpm")),
            Cadence = ParseNullableInt(el.Element(Ns + "Cadence"))
        };

        var timeText = (string?)el.Element(Ns + "Time");
        if (DateTime.TryParse(timeText, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            tp.Time = dt.ToLocalTime();

        var pos = el.Element(Ns + "Position");
        if (pos != null)
        {
            tp.LatitudeDegrees = ParseNullableDouble(pos.Element(Ns + "LatitudeDegrees"));
            tp.LongitudeDegrees = ParseNullableDouble(pos.Element(Ns + "LongitudeDegrees"));
        }

        // Speed from extensions
        var ext = el.Element(Ns + "Extensions") ?? el.Element("Extensions");
        if (ext != null)
        {
            var tpx = ext.Descendants().FirstOrDefault(e => e.Name.LocalName == "TPX");
            if (tpx != null)
            {
                var speedEl = tpx.Descendants().FirstOrDefault(e => e.Name.LocalName == "Speed");
                if (speedEl != null)
                    tp.Speed = ParseNullableDouble(speedEl);
            }
        }

        return tp;
    }

    private static XDocument BuildDocument(TcxDatabase db)
    {
        var root = new XElement(Ns + "TrainingCenterDatabase",
            new XAttribute("xmlns", Ns.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "xsi", Xsi.NamespaceName),
            new XAttribute(Xsi + "schemaLocation",
                "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 " +
                "http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd"),
            new XElement(Ns + "Activities",
                db.Activities.Select(BuildActivity)));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), root);
    }

    private static XElement BuildActivity(TcxActivity act)
    {
        var el = new XElement(Ns + "Activity",
            new XAttribute("Sport", act.Sport),
            new XElement(Ns + "Id", act.StartTimeUtc));

        if (!string.IsNullOrWhiteSpace(act.Notes))
            el.Add(new XElement(Ns + "Notes", act.Notes));

        foreach (var lap in act.Laps)
            el.Add(BuildLap(lap));

        return el;
    }

    private static XElement BuildLap(TcxLap lap)
    {
        var el = new XElement(Ns + "Lap",
            new XAttribute("StartTime", lap.StartTimeUtc),
            new XElement(Ns + "TotalTimeSeconds", lap.TotalTimeSeconds.ToString("F1", Inv)),
            new XElement(Ns + "DistanceMeters", lap.DistanceMeters.ToString("F2", Inv)),
            new XElement(Ns + "MaximumSpeed", lap.MaximumSpeed.ToString("F3", Inv)),
            new XElement(Ns + "Calories", lap.Calories));

        if (lap.AverageHeartRate.HasValue)
            el.Add(new XElement(Ns + "AverageHeartRateBpm",
                new XElement(Ns + "Value", lap.AverageHeartRate.Value)));

        if (lap.MaximumHeartRate.HasValue)
            el.Add(new XElement(Ns + "MaximumHeartRateBpm",
                new XElement(Ns + "Value", lap.MaximumHeartRate.Value)));

        el.Add(new XElement(Ns + "Intensity", lap.Intensity));
        el.Add(new XElement(Ns + "TriggerMethod", "Manual"));

        if (!string.IsNullOrWhiteSpace(lap.Notes))
            el.Add(new XElement(Ns + "Notes", lap.Notes));

        if (lap.Trackpoints.Count > 0)
        {
            var track = new XElement(Ns + "Track",
                lap.Trackpoints.Select(BuildTrackpoint));
            el.Add(track);
        }

        return el;
    }

    private static XElement BuildTrackpoint(TcxTrackpoint tp)
    {
        var el = new XElement(Ns + "Trackpoint",
            new XElement(Ns + "Time", tp.TimeUtc));

        if (tp.LatitudeDegrees.HasValue && tp.LongitudeDegrees.HasValue)
        {
            el.Add(new XElement(Ns + "Position",
                new XElement(Ns + "LatitudeDegrees", tp.LatitudeDegrees.Value.ToString("F7", Inv)),
                new XElement(Ns + "LongitudeDegrees", tp.LongitudeDegrees.Value.ToString("F7", Inv))));
        }

        if (tp.AltitudeMeters.HasValue)
            el.Add(new XElement(Ns + "AltitudeMeters", tp.AltitudeMeters.Value.ToString("F1", Inv)));

        if (tp.DistanceMeters.HasValue)
            el.Add(new XElement(Ns + "DistanceMeters", tp.DistanceMeters.Value.ToString("F2", Inv)));

        if (tp.HeartRateBpm.HasValue)
            el.Add(new XElement(Ns + "HeartRateBpm",
                new XElement(Ns + "Value", tp.HeartRateBpm.Value)));

        if (tp.Cadence.HasValue)
            el.Add(new XElement(Ns + "Cadence", tp.Cadence.Value));

        if (tp.Speed.HasValue)
        {
            el.Add(new XElement(Ns + "Extensions",
                new XElement(NsExt + "TPX",
                    new XAttribute("xmlns", NsExt.NamespaceName),
                    new XElement(NsExt + "Speed", tp.Speed.Value.ToString("F3", Inv)))));
        }

        return el;
    }

    private static double ParseDouble(XElement? el) =>
        double.TryParse((string?)el, NumberStyles.Any, Inv, out var v) ? v : 0;

    private static double? ParseNullableDouble(XElement? el) =>
        el != null && double.TryParse((string?)el, NumberStyles.Any, Inv, out var v) ? v : null;

    private static int ParseInt(XElement? el) =>
        int.TryParse((string?)el, out var v) ? v : 0;

    private static int? ParseNullableInt(XElement? el) =>
        el != null && int.TryParse((string?)el, out var v) ? v : null;

    private static int? ParseHeartRate(XElement? el) =>
        el == null ? null : ParseNullableInt(el.Element(Ns + "Value"));
}
