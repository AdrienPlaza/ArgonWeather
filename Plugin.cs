using BepInEx;

namespace Argon
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            gameObject.AddComponent<ArgonMenu>().Initialize(Config);
            gameObject.AddComponent<ArgonWeather.WeatherMenu>();
        }

        private void OnDestroy() => MenuApi.Reset();
    }

    public class PluginInfo
    {
        public const string GUID = "majorol.argon.weather";
        public const string Name = "ArgonWeather";
        public const string Version = "1.0.0";
    }
}
