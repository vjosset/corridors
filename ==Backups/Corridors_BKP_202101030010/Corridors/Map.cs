using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    public class Map
    {
        public List<Module> Modules { get; set; } = new List<Module>();

        [JsonIgnore]
        public List<Module> SelectedModules
        {
            get
            {
                return this.Modules.Where(m => m.IsSelected).ToList();
            }
        }

        public bool IsTileOccupied(Point tilePos)
        {
            return this.Modules.Any(mod => mod.ContainsTile(tilePos));
        }

        public Module GetModuleAt(Point tilePos)
        {
            return this.Modules.FirstOrDefault(mod => mod.ContainsTile(tilePos));
        }
    }
}
