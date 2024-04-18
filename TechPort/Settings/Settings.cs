using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TechPort.Helpers;

namespace TechPort.Settings
{
    public class DeskSettings
    {
        public string deskName = "Bureau a Victor";
        public List<uint> SavedPositions = new List<uint> { 69, 112 };

        public KeysShortCuts keysShortCuts = new KeysShortCuts();
        public class KeysShortCuts
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public Keys keyUp = Keys.Up;

            [JsonConverter(typeof(StringEnumConverter))]
            public Keys keyDown = Keys.Down;

            [JsonConverter(typeof(StringEnumConverter))]
            public KeyModifiers keyModifier = KeyModifiers.Alt;
        }
    }
}
