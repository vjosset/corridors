using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    public class Preferences
    {
        /// <summary>
        /// Gets or sets the path to the folder which contains the user's maps.
        /// </summary>
        public string PathToMaps { get; set; }

        /// <summary>
        /// Gets or sets the path to the folder which contains the ModulePacks.
        /// </summary>
        public string PathToModulePacks { get; set; }

        /// <summary>
        /// Gets or sets the color of the lines on the grid.
        /// </summary>
        public Color GridLineColor { get; set; } = Color.LightGray;

        /// <summary>
        /// Gets or sets the color of the major lines on the grid.
        /// </summary>
        public Color GridMajorLineColor { get; set; } = Color.Gray;

        /// <summary>
        /// Gets or sets the color of the grid's background.
        /// </summary>
        public Color GridBackgroundColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the period of major grid lines (in number of tiles).
        /// </summary>
        public int GridMajorCount = 5;

        /// <summary>
        /// Gets or sets the available module packs.
        /// </summary>
        [JsonIgnore]
        public List<ModulePack> ModulePacks { get; set; } = new List<ModulePack>();

        /// <summary>
        /// Loads the module packs defined in PathToModulePacks.
        /// </summary>
        public void LoadModulePacks()
        {
            this.ModulePacks.Clear();

            // Load the root folder for all modules, one subfolder per pack
            DirectoryInfo dir = new DirectoryInfo(this.PathToModulePacks);
            foreach (DirectoryInfo sd in dir.EnumerateDirectories())
            {
                // Load this module pack
                ModulePack pack = ModulePack.LoadModuleDefinition(sd.FullName);
                if (pack != null)
                {
                    // Loaded successfully
                    this.ModulePacks.Add(pack);
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Preferences()
        {
            this.PathToModulePacks = Utils.GetExecutableFolder() + "\\ModulePacks";
            this.PathToMaps = Utils.GetExecutableFolder() + "\\Maps";
        }

        /// <summary>
        /// Gets the path where preferences are stored and saved.
        /// </summary>
        public static string PreferenceFilePath
        {
            get
            {
                return Utils.GetExecutableFolder() + "\\preferences.json";
            }
        }

        /// <summary>
        /// Overwrites the current preference file with the content of this Preferences object.
        /// </summary>
        public void SavePreferences()
        {
            string json = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(PreferenceFilePath, json);
        }

        public static Preferences LoadPreferences()
        {
            return JsonConvert.DeserializeObject<Preferences>(System.IO.File.ReadAllText(PreferenceFilePath));
        }
    }
}
