namespace HomeController.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Q42.HueApi;
    using Q42.HueApi.Interfaces;

    public class HueLightingController : IHomeController
    {
        private static readonly Dictionary<string, IList<string>> RoomLightMappings = new Dictionary<string, IList<string>>
        {
            { "Kitchen", new List<string> { "1", "3" } },
            { "Hallway", new List<string> { "2", "4" } },
            { "Living", new List<string> { "5", "6", "7", "8", "9" } }
        };

        private readonly Dictionary<string, Func<Task>> handlers = new Dictionary<string, Func<Task>>();
        private ILocalHueClient localHueClient;

        public HueLightingController()
        {
            foreach (var mapping in RoomLightMappings)
            {
                var roomName = mapping.Key;
                var numberOfLights = mapping.Value.Count;

                this.handlers.Add(GetRoomNamePhrase(roomName), () => this.On(roomName));
                this.handlers.Add(GetRoomNamePhrase(roomName) + " off", () => this.Off(roomName));

                foreach (var gradient in ColorGradients.Gradients)
                {
                    var gradientName = gradient.Key;
                    var targetColors = gradient.Value.GetColorsForLights(numberOfLights);

                    this.handlers.Add(GetRoomNamePhrase(roomName) + " " + gradientName,
                        () => this.On(roomName, targetColors));
                }
            }
        }

        public async Task InitializeController()
        {
            var locator = new HttpBridgeLocator();
            var bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
            this.localHueClient = new LocalHueClient(bridgeIPs.First());

            var localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["HueAppKey"];
            string appKey;

            if (value == null)
            {
                appKey = await this.localHueClient.RegisterAsync("HomeControllerApp", "HomeController");
                localSettings.Values.Add("HueAppKey", appKey);
            }
            else
            {
                appKey = (string)value;
            }

            this.localHueClient.Initialize(appKey);
        }

        public IEnumerable<string> GetCommandPhrases()
        {
            return handlers.Keys;
        }

        public async Task ProcessCommandPhrase(string phraseText)
        {
            Func<Task> command;
            if (this.handlers.TryGetValue(phraseText, out command))
            {
                await command();
            }
        }

        private async Task On(string roomName, IList<RGBColor> colors = null)
        {
            if (colors == null)
            {
                var command = new LightCommand { On = true };
                await this.localHueClient.SendCommandAsync(command, RoomLightMappings[roomName]);
            }
            else
            {
                var lights = RoomLightMappings[roomName];
                var tasks = new List<Task>();

                for (var i = 0 ; i < lights.Count; i++)
                {
                    var command = new LightCommand { On = true };
                    command.SetColor(colors[i], "LCT001");
                    tasks.Add(this.localHueClient.SendCommandAsync(command, new List<string>() { lights[i] }));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task Off(string roomName)
        {
            var command = new LightCommand { On = false };
            await this.localHueClient.SendCommandAsync(command, RoomLightMappings[roomName]);
        }
        
        private static string GetRoomNamePhrase(string roomName)
        {
            return roomName.ToLower() + " lights";
        }
    }
}