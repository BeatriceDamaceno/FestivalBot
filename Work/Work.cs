using System;
using System.Collections.Generic;
using System.Linq;

namespace FestivalBot.Work
{
    [Flags]
    public enum WorkDays
    {
        None = 0,
        Monday = 1 << 0,
        Tuesday = 1 << 1,
        Wednesday = 1 << 2,
        Thursday = 1 << 3,
        Friday = 1 << 4,
        Saturday = 1 << 5,
        Sunday = 1 << 6,
        AnyDay = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,
        MonWed = Monday | Tuesday | Wednesday,
        MonFri = Monday | Tuesday | Wednesday | Thursday | Friday,
        WedFri = Wednesday | Thursday | Friday,
        SatSun = Saturday | Sunday,
        MonFriSat = Monday | Friday | Saturday,
        TueThuSat = Tuesday | Thursday | Saturday
    }

    [Flags]
    public enum WorkTime
    {
        None = 0,
        Daytime = 1 << 0,
        Evening = 1 << 1,
        Night = 1 << 2,
        DayOrNight = Daytime | Night
    }

    public sealed class Workplace
    {
        public string Location { get; }
        public string Name { get; }
        public WorkDays Days { get; }
        public WorkTime Time { get; }
        /// <summary>Base pay already divided by 100 (Macca/yen unit shown in the table).</summary>
        public int BasePay { get; }
        /// <summary>True when pay is listed with a "+" (variable / can be higher).</summary>
        public bool VariablePay { get; }

        public Workplace(string location, string name, WorkDays days, WorkTime time, int basePay, bool variablePay = false)
        {
            Location = location;
            Name = name;
            Days = days;
            Time = time;
            BasePay = basePay;
            VariablePay = variablePay;
        }

        public string BasePayDisplay => VariablePay ? $"¥{BasePay}+" : $"¥{BasePay}";

        public string DaysDisplay
        {
            get
            {
                if (Days == WorkDays.AnyDay) return "Any day";
                if (Days == WorkDays.MonWed) return "Mon–Wed";
                if (Days == WorkDays.MonFri) return "Mon–Fri";
                if (Days == WorkDays.WedFri) return "Wed–Fri";
                if (Days == WorkDays.SatSun) return "Sat–Sun";
                if (Days == WorkDays.MonFriSat) return "Mon/Fri/Sat";
                if (Days == WorkDays.TueThuSat) return "Tue/Thu/Sat";
                return Days.ToString();
            }
        }

        public string TimeDisplay
        {
            get
            {
                if (Time == WorkTime.DayOrNight) return "Day/Night";
                if (Time == WorkTime.Daytime) return "Daytime";
                if (Time == WorkTime.Evening) return "Evening";
                if (Time == WorkTime.Night) return "Night";
                return Time.ToString();
            }
        }

        public override string ToString() =>
            $"{Location} | {Name} | {DaysDisplay} | {TimeDisplay} | {BasePayDisplay}";
    }

    public static class Workplaces
    {
        public static readonly IReadOnlyList<Workplace> All = new List<Workplace>
        {
            new Workplace("Paulownia Mall", "Chagall Café", WorkDays.MonWed, WorkTime.Evening, 25),
            new Workplace("Port Island Station", "Screen Shot", WorkDays.SatSun, WorkTime.Daytime, 50),
            new Workplace("Paulownia Mall", "Be Blue V", WorkDays.MonFri, WorkTime.Daytime, 35),
            new Workplace("South Shopping District", "Daycare Assistant", WorkDays.MonFriSat, WorkTime.Daytime, 40, variablePay: true),
            // Shiroku Pub lists "Evening" for the day column — treat as any day, evening shift
            new Workplace("Shiroku Pub", "Dishwasher", WorkDays.AnyDay, WorkTime.Evening, 15, variablePay: true),
            new Workplace("Hospital", "Hospital Janitor", WorkDays.WedFri, WorkTime.Evening, 50, variablePay: true),
            new Workplace("North Shopping District", "Tutor", WorkDays.TueThuSat, WorkTime.Evening, 100),
            new Workplace("Shibuya — Central Street", "Triple Seven", WorkDays.AnyDay, WorkTime.Daytime, 35),
            new Workplace("Shibuya — Underground Mall", "Rafflesia", WorkDays.AnyDay, WorkTime.DayOrNight, 32),
            new Workplace("Shinjuku", "Crossroads", WorkDays.AnyDay, WorkTime.Evening, 72),
        };

        public static IEnumerable<Workplace> AvailableOn(WorkDays day, WorkTime time) =>
            All.Where(w => (w.Days & day) != 0 && (w.Time & time) != 0);

        public static WorkDays ToWorkDay(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => WorkDays.Monday,
            DayOfWeek.Tuesday => WorkDays.Tuesday,
            DayOfWeek.Wednesday => WorkDays.Wednesday,
            DayOfWeek.Thursday => WorkDays.Thursday,
            DayOfWeek.Friday => WorkDays.Friday,
            DayOfWeek.Saturday => WorkDays.Saturday,
            DayOfWeek.Sunday => WorkDays.Sunday,
            _ => WorkDays.None
        };

        /// <summary>
        /// Daytime 06:00–17:59, Evening 18:00–21:59, Night 22:00–05:59 (local time).
        /// </summary>
        public static WorkTime ToWorkTime(DateTime localNow)
        {
            int hour = localNow.Hour;
            if (hour >= 6 && hour < 18) return WorkTime.Daytime;
            if (hour >= 18 && hour < 22) return WorkTime.Evening;
            return WorkTime.Night;
        }

        public static IReadOnlyList<Workplace> AvailableNow(DateTime localNow) =>
            AvailableOn(ToWorkDay(localNow.DayOfWeek), ToWorkTime(localNow)).ToList();

        public static string AsTable()
        {
            var lines = new List<string>
            {
                "Location | Workplace | Day | Time | Base pay ÷ 100",
                "---------|-----------|-----|------|---------------"
            };

            foreach (var w in All)
                lines.Add($"{w.Location} | {w.Name} | {w.DaysDisplay} | {w.TimeDisplay} | {w.BasePayDisplay}");

            return string.Join(Environment.NewLine, lines);
        }
    }
}
