using System;
using Argon;
using UnityEngine;

namespace ArgonWeather
{
    internal enum TimeChoice { Game, Morning, TenAM, Day, Evening, Night }

    /// <summary>Page "Customisation" du menu Argon : heure epinglee + pluie (API native du jeu).</summary>
    internal sealed class WeatherMenu : MonoBehaviour
    {
        internal const string RootLabel = "Customisation";

        // Index exacts utilises par Time Changer (Gorilla.Time V2.1).
        private static readonly int[] TimeIndex = { -1, 1, 3, 4, 6, 0 };

        private TimeChoice time = TimeChoice.Game;
        private bool rain;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            MenuApi.RegisterRootItem(RootLabel, CreateRootPage);
        }

        private MenuPage CreateRootPage() => new MenuPage(RootLabel, new[]
        {
            new MenuItem("Heure : " + TimeLabel(time), CreateTimePage),
            new MenuItem("Pluie : " + (rain ? "ON" : "OFF"), ToggleRain),
        });

        private MenuPage CreateTimePage() => new MenuPage("Heure du jour", new[]
        {
            TimeItem("Normal (jeu)", TimeChoice.Game),
            TimeItem("Matin", TimeChoice.Morning),
            TimeItem("10h", TimeChoice.TenAM),
            TimeItem("Jour", TimeChoice.Day),
            TimeItem("Soir", TimeChoice.Evening),
            TimeItem("Nuit", TimeChoice.Night),
        });

        private MenuItem TimeItem(string label, TimeChoice choice) =>
            new MenuItem(Mark(label, time == choice), () => { time = choice; ApplyTime(); });

        private static string Mark(string label, bool selected) =>
            selected ? "<color=#ffc600>" + label + "</color>" : label;

        private static string TimeLabel(TimeChoice t)
        {
            switch (t)
            {
                case TimeChoice.Game: return "Normal (jeu)";
                case TimeChoice.Morning: return "Matin";
                case TimeChoice.TenAM: return "10h";
                case TimeChoice.Day: return "Jour";
                case TimeChoice.Evening: return "Soir";
                default: return "Nuit";
            }
        }

        private void ApplyTime()
        {
            try
            {
                var manager = BetterDayNightManager.instance;
                if (manager == null) return;
                if (time == TimeChoice.Game)
                {
                    manager.UnsetTimeIndexOverrideFunction();
                    manager.ClearTimeOfDay(true);
                    return;
                }
                int index = TimeIndex[(int)time];
                // Epingle l'heure en permanence (survit aux resyncs serveur), puis saute a l'heure choisie.
                manager.SetTimeIndexOverrideFunction(current => index);
                manager.SetTimeOfDay(index, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private void ToggleRain()
        {
            rain = !rain;
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
    }
}
