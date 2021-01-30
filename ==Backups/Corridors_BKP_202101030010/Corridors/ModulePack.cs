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

                // Update modules in this pack with absolute path for images
                foreach (Module mod in pack.Categories.Values.SelectMany(m => m))
                {
                    for (int i = 0; i < mod.Images.Length; i++)
                    {
                        mod.Images[i] = path + "\\" + mod.Images[i];
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
