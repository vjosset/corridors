using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    /// <summary>
    /// Represents a Module archetype (as defined in ModulePacks).
    /// </summary>
    public class Module
    {
        const int ThumbnailSize = 100;

        /// <summary>
        /// Private backing field for the "Name" property.
        /// </summary>
        private string _name = "";

        /// <summary>
        /// Gets or sets the name of this module.
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._name))
                {
                    // No name defined for this module - Use the file name
                    int fileNameStart = this.ImagePath.LastIndexOf("\\") + 1;
                    int fileNameEnd = this.ImagePath.LastIndexOf(".");
                    this._name = this.ImagePath.Substring(fileNameStart, fileNameEnd - fileNameStart);
                }
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        /// <summary>
        /// Gets the unique key for this module. Used for map references.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the width (in tiles) of this module.
        /// </summary>
        public int Width { get; set; } = 1;

        /// <summary>
        /// Gets or sets the height (in tiles) of this module.
        /// </summary>
        public int Height { get; set; } = 1;

        /// <summary>
        /// Gets or sets the image variations of this module.
        /// +-------+-------+-----------+
        /// |Index  |Flip   |Rotation   |
        /// +-------+-------+-----------+
        /// |0, 0   |None   |0          |
        /// |0, 1   |None   |90 CW      |
        /// |0, 2   |None   |180        |
        /// |0, 3   |None   |270 CW     |
        /// |1, 0   |H      |0          |
        /// |1, 1   |H      |90 CW      |
        /// |1, 2   |H      |180        |
        /// |1, 3   |H      |270 CW     |
        /// |2, 0   |V      |0          |
        /// |2, 1   |V      |90 CW      |
        /// |2, 2   |V      |180        |
        /// |2, 3   |V      |270 CW     |
        /// |3, 0   |HV     |0          |
        /// |3, 1   |HV     |90 CW      |
        /// |3, 2   |HV     |180        |
        /// |3, 3   |HV     |270 CW     |
        /// +-------+-------+-----------+
        /// </summary>
        public Image[,] Images { get; private set; } = new Image[4, 4];

        /// <summary>
        /// Gets the Image object for this module's thumbnail (to be displayed in ListView).
        /// </summary>
        public Image Thumbnail { get; private set; } = new Bitmap(ThumbnailSize, ThumbnailSize);

        /// <summary>
        /// Gets the path to the image for this Module.
        /// </summary>
        public string ImagePath { get; set; } = "";

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Module() { }

        /// <summary>
        /// Loads the module's image and builds the variations for it.
        /// </summary>
        /// <param name="filePath">The (absolute) path to this module's image.</param>
        public void LoadImage()
        {
            if (!string.IsNullOrEmpty(this.ImagePath))
            {
                // Load the original bitmap
                Image orig = Bitmap.FromFile(this.ImagePath);

                // Resize the original bitmap to its maximum resolution of 100 pixels per tile
                const int MaxImageSize = 100;
                if (orig.Width > this.Width * MaxImageSize || orig.Height > this.Height * MaxImageSize)
                {
                    // The original bitmap is higher resolution than we will need, make it smaller
                    Image temp = new Bitmap(this.Width * MaxImageSize, this.Height * MaxImageSize);
                    using (Graphics gResize = Graphics.FromImage(temp))
                    {
                        gResize.DrawImage(orig, new Rectangle(0, 0, temp.Width, temp.Height), new Rectangle(0, 0, orig.Width, orig.Height), GraphicsUnit.Pixel);
                    }
                    orig = temp;
                }

                // Build all the rotations/flips of the original image
                Image[] variations = new Image[8];
                for (int i = 0; i < 8; i++)
                {
                    variations[i] = (Image)orig.Clone();
                    variations[i].RotateFlip((RotateFlipType)i);
                }

                // Now assign the variations to our matrix of images
                // See the definition of RotateFlipType - RotateNoneFlipNone is the same as Rotate180FlipXY
                Images[0, 0] = variations[(int)RotateFlipType.RotateNoneFlipNone];
                Images[0, 1] = variations[(int)RotateFlipType.Rotate90FlipNone];
                Images[0, 2] = variations[(int)RotateFlipType.Rotate180FlipNone];
                Images[0, 3] = variations[(int)RotateFlipType.Rotate270FlipNone];

                Images[1, 0] = variations[(int)RotateFlipType.RotateNoneFlipX];
                Images[1, 1] = variations[(int)RotateFlipType.Rotate90FlipX];
                Images[1, 2] = variations[(int)RotateFlipType.Rotate180FlipX];
                Images[1, 3] = variations[(int)RotateFlipType.Rotate270FlipX];

                Images[2, 0] = variations[(int)RotateFlipType.RotateNoneFlipY];
                Images[2, 1] = variations[(int)RotateFlipType.Rotate90FlipY];
                Images[2, 2] = variations[(int)RotateFlipType.Rotate180FlipY];
                Images[2, 3] = variations[(int)RotateFlipType.Rotate270FlipY];

                Images[3, 0] = variations[(int)RotateFlipType.RotateNoneFlipXY];
                Images[3, 1] = variations[(int)RotateFlipType.Rotate90FlipXY];
                Images[3, 2] = variations[(int)RotateFlipType.Rotate180FlipXY];
                Images[3, 3] = variations[(int)RotateFlipType.Rotate270FlipXY];

                // Load the thumbnail for this module
                // Target width, target height, the target x and y
                int tw, th, tx, ty;

                int w = orig.Width;
                int h = orig.Height;

                //get the ratio of width / height
                double whRatio = (double)w / h;

                //if width >= height
                if (orig.Width >= orig.Height)
                {
                    //set the width to [size] and compute the height
                    tw = ThumbnailSize;
                    th = (int)(tw / whRatio);
                }
                else
                {
                    //otherwise set the height to [size] and compute the width
                    th = ThumbnailSize;
                    tw = (int)(th * whRatio);
                }

                //now we compute where in our final image we're going to draw it
                tx = (ThumbnailSize - tw) / 2;
                ty = (ThumbnailSize - th) / 2;

                //create our final image - you can set the PixelFormat to anything that suits
                this.Thumbnail = new Bitmap(ThumbnailSize, ThumbnailSize, PixelFormat.Format24bppRgb);

                //get a Graphics from this final image
                Graphics g = Graphics.FromImage(this.Thumbnail);

                //clear the final image to white (or anything else) for the areas that the input image doesn't cover
                g.Clear(Color.White);
                
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                //and here is where everything happens
                //the first Rectangle is where (tx, ty) and what size (tw, th) we want to draw the image
                //the second Rectangle is the portion of the image we want to draw, in this case all of it
                //the drawing routine does the resizing

                g.DrawImage(orig, new Rectangle(tx, ty, tw, th), new Rectangle(0, 0, w, h), GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Indicates a rotation angle.
        /// </summary>
        public enum Rotation
        {
            Rotate0,
            Rotate90,
            Rotate180,
            Rotate270
        }

        /// <summary>
        /// Indicates a mirrored image type.
        /// </summary>
        public enum Mirror
        {
            MirrorNone,
            MirrorX,
            MirrorY,
            MirrorXY
        }
    }
}
