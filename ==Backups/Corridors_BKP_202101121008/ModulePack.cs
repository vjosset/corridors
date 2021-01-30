using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corridors
{
    public class ModulePack
    {
        public string Name { get; set; } = "[Unnamed]";
        public string RootPath { get; set; } = "";

        public Dictionary<string, Module[]> Categories = new Dictionary<string, Module[]>();

        public ModulePack()
        {

        }

        public static ModulePack LoadModuleDefinition(string path)
        {
            try
            {
                ModulePack pack = JsonConvert.DeserializeObject<ModulePack>(System.IO.File.ReadAllText(path + "\\module.json"));
                pack.RootPath = path;

                // Update modules in this pack with absolute path for images and load the images
                foreach (string key in pack.Categories.Keys)
                {
                    Module[] category = pack.Categories[key];

                    foreach (Module mod in category)
                    {
                        // Set the absolute path
                        mod.ImagePath = path + "\\" + mod.ImagePath;

                        // Load the image variations
                        mod.LoadImage();

                        // Set the key
                        if (string.IsNullOrWhiteSpace(mod.Key))
                        {
                            // No key defined in the ModulePack; use the pack, category, and module name
                            mod.Key = pack.Name + "." + key + "." + mod.Name;
                        }
                    }
                }

                // Done
                return pack;
            }
            catch(Exception ex)
            {
                // Could not load this module
                MessageBox.Show("Could not load the module at \"" + path + "\": \r\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
