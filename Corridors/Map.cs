using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    [Serializable]
    public class Map
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Map()
        {
            this.Layers = new List<MapLayer>();
        }

        public string MapName { get; set; } = "New Map";
        public string FileName { get; set; } = "";

        /// <summary>
        /// Gets or sets the layers on this map.
        /// </summary>
        public List<MapLayer> Layers { get; set; } = new List<MapLayer>();

        /// <summary>
        /// Gets all the modules across all layers of this map.
        /// </summary>
        public List<MapModule> AllModules => this.Layers.SelectMany(lay => lay.Modules).ToList();
    }
}
