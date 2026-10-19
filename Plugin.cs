using BepInEx;
using Utilla.Attributes;

namespace Argon
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla")]
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            gameObject.AddComponent<ArgonMenu>().Initialize(Config);
            gameObject.AddComponent<ArgonWeather.WeatherMenu>().Initialize(Config);
        }

        [ModdedGamemodeJoin]
        public void OnModdedJoin(string gamemode) => ArgonWeather.WeatherMenu.SetModded(true);

        [ModdedGamemodeLeave]
        public void OnModdedLeave(string gamemode)
        {
            // WalkSimulator-style guard: never drop back into a public lobby with the mod active.
            if (NetworkSystem.Instance != null) NetworkSystem.Instance.ReturnToSinglePlayer();
            ArgonWeather.WeatherMenu.SetModded(false);
        }

        private void OnDestroy() => MenuApi.Reset();
    }

    public class PluginInfo
    {
        public const string GUID = "majorol.argon.weather";
        public const string Name = "ArgonWeather";
        public const string Version = "1.3.0";
    }
}
