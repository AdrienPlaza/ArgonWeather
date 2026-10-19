using System;
using Argon;
using BepInEx.Configuration;
using UnityEngine;

namespace ArgonWeather
{
    /// <summary>"Customisation" page of the Argon menu: pinned time of day + rain (native game API).</summary>
    internal sealed class WeatherMenu : MonoBehaviour
    {
        internal const string RootLabel = "Customisation";

        // Preset indices, identical to the ones used by Time Changer (Gorilla.Time V2.1).
        private static readonly int[] PresetIndex = { 1, 3, 4, 6, 0 };
        private static readonly string[] PresetName = { "Morning", "10 AM", "Day", "Evening", "Night" };

        private ConfigEntry<int> hourConfig;
        private ConfigEntry<bool> rainConfig;
        private int hourIndex = -1;
        private bool rain;
        private BetterDayNightManager pinned;

        internal void Initialize(ConfigFile config)
        {
            hourConfig = config.Bind("Weather", "HourIndex", -1, "Pinned time of day index (-1 = normal game time).");
            rainConfig = config.Bind("Weather", "Rain", false, "Rain on/off.");
            hourIndex = hourConfig.Value < -1 ? -1 : hourConfig.Value;
            rain = rainConfig.Value;
            MenuApi.RegisterRootItem(RootLabel, CreateRootPage);
        }

        private void Awake() => DontDestroyOnLoad(gameObject);

        private MenuPage CreateRootPage()
        {
            Reapply();
            return new MenuPage(RootLabel, new[]
            {
                new MenuItem("Time: " + HourLabel(hourIndex), CreateTimePage),
                new MenuItem("Rain: " + (rain ? "ON" : "OFF"), ToggleRain),
                new MenuItem("Reset everything", ResetAll),
            });
        }

        private MenuPage CreateTimePage()
        {
            var items = new System.Collections.Generic.List<MenuItem>
            {
                TimeItem("Normal (game)", -1),
            };
            for (int i = 0; i < PresetIndex.Length; i++)
                items.Add(TimeItem(PresetName[i], PresetIndex[i]));
            items.Add(new MenuItem("- 1 hour", () => Shift(-1)));
            items.Add(new MenuItem("+ 1 hour", () => Shift(1)));
            return new MenuPage("Time of day", items);
        }

        private MenuItem TimeItem(string label, int index) =>
            new MenuItem(Mark(label + HourSuffix(index), hourIndex == index), () => SetHour(index));

        private static string Mark(string label, bool selected) =>
            selected ? "<color=#ffc600>" + label + "</color>" : label;

        private string HourLabel(int index)
        {
            if (index < 0) return "Normal (game)";
            for (int i = 0; i < PresetIndex.Length; i++)
                if (PresetIndex[i] == index) return PresetName[i] + HourSuffix(index);
            return "Custom" + HourSuffix(index);
        }

        private static string HourSuffix(int index)
        {
            var manager = BetterDayNightManager.instance;
            if (manager == null) return "";
            string hour = HourText(manager, index);
            return hour == null ? "" : " (" + hour + ")";
        }

        private static string HourText(BetterDayNightManager manager, int index)
        {
            double[] range = manager.timeOfDayRange;
            if (range == null || range.Length == 0) return null;
            int i = ((index % range.Length) + range.Length) % range.Length;
            double hours = 0;
            for (int k = 0; k < i; k++) hours += range[k];
            return ((int)hours).ToString("00") + ":00";
        }

        private void SetHour(int index)
        {
            hourIndex = index;
            hourConfig.Value = index;
            ApplyTime();
        }

        private void Shift(int delta)
        {
            var manager = BetterDayNightManager.instance;
            if (manager == null) return;
            double[] range = manager.timeOfDayRange;
            if (range == null || range.Length == 0) return;
            int count = range.Length;
            int start = hourIndex >= 0 ? hourIndex : manager.currentTimeIndex;
            SetHour(((start + delta) % count + count) % count);
        }

        private void ApplyTime()
        {
            try
            {
                var manager = BetterDayNightManager.instance;
                if (manager == null) return;
                if (hourIndex < 0)
                {
                    manager.UnsetTimeIndexOverrideFunction();
                    manager.ClearTimeOfDay(true);
                    pinned = null;
                    return;
                }
                Pin(manager);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private void Pin(BetterDayNightManager manager)
        {
            int index = hourIndex;
            // Pins the time permanently (survives server resyncs), then jumps to the chosen hour.
            manager.SetTimeIndexOverrideFunction(current => index);
            manager.SetTimeOfDay(index, true);
            pinned = manager;
        }

        private void Reapply()
        {
            try
            {
                var manager = BetterDayNightManager.instance;
                if (manager == null) return;
                if (hourIndex >= 0 && (manager != pinned || manager.currentSetting != TimeSettings.Static || manager.currentTimeIndex != hourIndex))
                    Pin(manager);
                if (rain && manager.CurrentWeather() != BetterDayNightManager.WeatherType.Raining)
                    manager.SetFixedWeather(BetterDayNightManager.WeatherType.Raining, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private void ToggleRain()
        {
            rain = !rain;
            rainConfig.Value = rain;
            ApplyRain();
        }

        private void ApplyRain()
        {
            try
            {
                var manager = BetterDayNightManager.instance;
                if (manager == null) return;
                if (rain) manager.SetFixedWeather(BetterDayNightManager.WeatherType.Raining, true);
                else manager.ClearFixedWeather(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private void ResetAll()
        {
            hourIndex = -1;
            rain = false;
            hourConfig.Value = -1;
            rainConfig.Value = false;
            try
            {
                var manager = BetterDayNightManager.instance;
                if (manager == null) return;
                manager.UnsetTimeIndexOverrideFunction();
                manager.ClearTimeOfDay(true);
                manager.ClearFixedWeather(true);
                pinned = null;
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }
    }
}
