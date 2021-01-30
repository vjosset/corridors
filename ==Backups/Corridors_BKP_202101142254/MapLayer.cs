using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    public class MapLayer
    {
        public List<MapModule> Modules { get; set; } = new List<MapModule>();

        [JsonIgnore]
        public List<MapModule> SelectedModules
        {
            get
            {
                return this.Modules.Where(m => m.IsSelected).ToList();
            }
        }

        [JsonIgnore]
        public List<MapModule> ModulesToBeDeleted
        {
            get
            {
                return this.Modules.Where(m => m.IsToBeDeleted).ToList();
            }
        }

        public bool IsTileOccupied(Point tilePos)
        {
            return this.Modules.Any(mod => mod.ContainsTile(tilePos));
        }

        public bool IsRectangleOccupied(Rectangle rect)
        {
            return this.Modules.Any(mod => mod.Overlaps(rect));
        }

        public MapModule GetModuleAt(Point tilePos)
        {
            return this.Modules.FirstOrDefault(mod => mod.ContainsTile(tilePos));
        }
    }
}
