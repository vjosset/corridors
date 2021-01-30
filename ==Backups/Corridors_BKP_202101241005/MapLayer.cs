using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    [Serializable]
    public class MapLayer
    {
        /// <summary>
        /// Gets or sets this layer's name.
        /// </summary>
        public string Name { get; set; } = "Map";

        /// <summary>
        /// Gets or sets the MapModules on this MapLayer.
        /// </summary>
        public List<MapModule> Modules { get; set; } = new List<MapModule>();

        /// <summary>
        /// Gets or sets a boolean indicating whether this layer is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets a boolean indicating whether this layer should project shadows onto layers that are below it.
        /// </summary>
        public bool ShowShadows { get; set; } = true;

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
