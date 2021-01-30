using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corridors
{
    public static class Utils
    {
        private static Random rand = new Random();

        /// <summary>
        /// Shifts the characters in the provided string toward the right.
        /// Examples:
        /// * ShiftString("ABCD", 1) = "BCDA"
        /// * ShiftString("ABCD", 2) = "CDAB"
        /// </summary>
        /// <param name="str">The string to shift.</param>
        /// <param name="shift">The number of characters to shift.</param>
        /// <returns></returns>
        public static string ShiftString(string str, int shift)
        {
            return str.Substring(str.Length - shift, shift) + str.Substring(0, str.Length - shift);
        }

        /// <summary>
        /// Rotates the specified two-dimensional array clockwise.
        /// </summary>
        /// <param name="matrix">The two-dimensional array to rotate.</param>
        /// <returns></returns>
        public static Tile[,] RotateMatrix(Tile[,] matrix)
        {
            // Prepare the new tileset (flip the dimensions)
            Tile[,] newTiles = new Tile[matrix.GetLength(1), matrix.GetLength(0)];

            // Prepare the indices for the new tile coordinates
            int newCol, newRow = 0;

            // Loop through the old columns - Columns will become rows, and rows will become columns
            for (int oldCol = 0; oldCol < matrix.GetLength(1); oldCol++)
            {
                newCol = 0;
                // Loop through the old rows
                for (int oldRow = matrix.GetLength(0) - 1; oldRow >= 0; oldRow--)
                {
                    // Switch the positions (col/row) in the new tileset
                    newTiles[newRow, newCol] = matrix[oldRow, oldCol];

                    // Move to the next (new) column
                    newCol++;
                }
                newRow++;
            }

            // Update tiles to use our new tileset
            return newTiles;
        }

        /// <summary>
        /// Rotates the specified two-dimensional array clockwise.
        /// </summary>
        /// <param name="matrix">The two-dimensional array to rotate.</param>
        /// <returns></returns>
        public static MapModule[,] RotateMatrix(MapModule[,] matrix)
        {
            // Prepare the new tileset (flip the dimensions)
            MapModule[,] newModules = new MapModule[matrix.GetLength(1), matrix.GetLength(0)];

            // Prepare the indices for the new tile coordinates
            int newCol, newRow = 0;

            // Loop through the old columns - Columns will become rows, and rows will become columns
            for (int oldCol = 0; oldCol < matrix.GetLength(1); oldCol++)
            {
                newCol = 0;
                // Loop through the old rows
                for (int oldRow = matrix.GetLength(0) - 1; oldRow >= 0; oldRow--)
                {
                    MapModule mod = matrix[oldRow, oldCol];

                    // Switch the positions (col/row) in the new tileset
                    newModules[newRow, newCol] = mod;

                    if (mod != null)
                    {
                        mod.Position = new System.Drawing.Point(newCol, newRow);
                    }

                    // Move to the next (new) column
                    newCol++;
                }
                newRow++;
            }

            // Update tiles to use our new tileset
            return newModules;
        }

        /// <summary>
        /// Rotates the specified two-dimensional array clockwise.
        /// </summary>
        /// <param name="matrix">The two-dimensional array to rotate.</param>
        /// <returns></returns>
        public static MapModule[,] RotateMatrixCW(MapModule[,] matrix, Point offset)
        {
            int origWidth = matrix.GetLength(0);
            int origHeight = matrix.GetLength(1);

            // Prepare the new tileset (flip the dimensions)
            MapModule[,] newModules = new MapModule[origHeight, origWidth];

            for (int row = 0; row < origHeight; row++)
            {
                for (int col = 0; col < origWidth; col++)
                {
                    MapModule mod = matrix[origWidth - col - 1, row];
                    if (mod != null)
                    {
                        newModules[row, col] = mod;

                        // Update the MapModule's position with the offset
                        mod.Position = new Point(col + offset.X, row + offset.Y);

                        // Rotate the MapModule itself
                        mod.RotateCW();
                    }
                }
            }

            // Update tiles to use our new tileset
            return newModules;
        }

        /// <summary>
        /// Rotates the specified two-dimensional array counter-clockwise.
        /// </summary>
        /// <param name="matrix">The two-dimensional array to rotate.</param>
        /// <returns></returns>
        public static MapModule[,] RotateMatrixCCW(MapModule[,] matrix, Point offset)
        {
            int origWidth = matrix.GetLength(0);
            int origHeight = matrix.GetLength(1);

            // Prepare the new tileset (flip the dimensions)
            MapModule[,] newModules = new MapModule[origHeight, origWidth];

            for (int row = 0; row < origHeight; row++)
            {
                for (int col = 0; col < origWidth; col++)
                {
                    MapModule mod = matrix[col, origHeight - row - 1];
                    if (mod != null)
                    {
                        newModules[row, col] = mod;

                        // Update the MapModule's position with the offset
                        mod.Position = new Point(col + offset.X, row + offset.Y);

                        // Rotate the MapModule itself
                        mod.RotateCCW();
                    }
                }
            }

            // Update tiles to use our new tileset
            return newModules;
        }

        /// <summary>
        /// Returns the path to the current executable.
        /// </summary>
        /// <returns></returns>
        public static string GetPathToExecutable()
        {
            return System.Reflection.Assembly.GetEntryAssembly().Location;
        }
        
        /// <summary>
        /// Returns the path to the current executable's parent folder.
        /// </summary>
        /// <returns></returns>
        public static string GetExecutableFolder()
        {
            string exePath = GetPathToExecutable();
            return exePath.Substring(0, exePath.LastIndexOf("\\"));
        }

        public static dynamic GetRandomArrayMember(dynamic[] array)
        {
            return array[rand.Next(0, array.Length)];
        }

        public static Rectangle TileRectangleToPixelRectangle(Rectangle tileRect, int scale)
        {
            Rectangle pixelRect = new Rectangle(tileRect.X * scale, tileRect.Y * scale, tileRect.Width * scale, tileRect.Height * scale);
            return pixelRect;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        ///     Performance tracker class used to track time taken and number of occurrences of different processes in your code.
        ///     Usage:
        ///     * Create a new PerfTracker object for the process whose performance you want to measure.
        ///     * In the code for your process, wrap each step of the process between calls to "myperftracker.StartStack("Step 1")"
        ///     and "myperftracker.StopStack("Step 2")"
        ///     * At the end of your process, print out the summary of the time taken for each step using
        ///     "myperftracker.SummaryTree()".
        /// </summary>
        [Serializable]
        public class PerfTracker
        {
            #region Members

            /// <summary>
            ///     Gets or sets a value indicating whether PerfTrackers are disabled globally (static).
            /// </summary>
            public static bool Disabled { get; set; }

            /// <summary>
            /// Gets the time the perftracker was created. (Used to sort it later).
            /// </summary>
            public DateTime CreationTime { get; private set; }

            /// <summary>The Lock object used for synchronizing.</summary>
            private readonly object _objMyLock = new object();

            /// <summary>
            /// Indicates whether a StartStack was called and not stopped with StopStack (PerfTracker in progress).
            /// </summary>
            private bool _blnIsRunning;

            /// <summary>
            /// The detail for this PerfTracker (child PerfTrackers).
            /// </summary>
            private ConcurrentDictionary<string, PerfTracker> _colDetail = new ConcurrentDictionary<string, PerfTracker>();

            /// <summary>
            /// The total number of calls for this PerfTracker.
            /// </summary>
            private int _intCount;

            /// <summary>
            /// The total number of mismatched stop calls (StopStack doesn't match StartStack) for this PerfTracker.
            /// </summary>
            private int _intEmptyStopCalls;

            /// <summary>
            ///     The int mismatch stop calls.
            /// </summary>
            private int _intMismatchStopCalls;

            /// <summary>
            /// The total number of missing stop calls (StartCalls - StopCalls) for this PerfTracker.
            /// </summary>
            private int _intMissingStopCalls;

            /// <summary>
            /// The total number of start calls (StartStack) for this PerfTracker.
            /// </summary>
            private int _intStartCount;

            /// <summary>
            /// The total number of stop calls (StopStack) for this PerfTracker.
            /// </summary>
            private int _intStopCount;

            /// <summary>
            /// The minimum running time (in ticks) for this PerfTracker.
            /// </summary>
            private long _lngMaxTicks;

            /// <summary>
            /// The minimum running time (in ticks) for this PerfTracker.
            /// </summary>
            private long _lngMinTicks = long.MaxValue;

            /// <summary>
            /// The start time (in ticks) for this PerfTracker.
            /// </summary>
            private long _lngStart;

            /// <summary>
            /// The total time spent (in ticks) for this PerfTracker.
            /// </summary>
            private long _lngTotalTicks;

            /// <summary>
            /// The current PerfTracker.
            /// </summary>
            private PerfTracker _ptrCurrent;

            /// <summary>
            /// This PerfTracker's parent PerfTracker.
            /// </summary>
            private PerfTracker _ptrParent;

            #endregion

            #region Properties

            /// <summary>
            ///     Gets or sets the name of this performance tracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            ///     Gets the total running time of this performance tracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public double TotalSeconds
            {
                get
                {
                    if (Disabled)
                    {
                        return -1;
                    }

                    if (this._ptrParent == null)
                    {
                        return this._colDetail != null && this._colDetail.Count > 0
                                   ? Math.Round(this._colDetail.Sum(subTracker => subTracker.Value.TotalSeconds), 3)
                                   : 0;
                    }

                    return Math.Round(new TimeSpan(this._lngTotalTicks).TotalSeconds, 3);
                }
            }

            /// <summary>
            ///     Gets the total number of occurrences for this Performance Tracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public int Count
            {
                get
                {
                    if (Disabled)
                    {
                        return -1;
                    }

                    if (this._ptrParent == null)
                    {
                        return 0;
                    }

                    return this._intCount;
                }
            }

            /// <summary>
            ///     Gets the average running time of this performance tracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public double AvgSeconds
            {
                get
                {
                    if (Disabled)
                    {
                        return -1;
                    }

                    if (this._ptrParent == null)
                    {
                        return this._colDetail != null && this._colDetail.Count > 0
                                   ? Math.Round(this._colDetail.Average(subTracker => subTracker.Value.AvgSeconds), 3)
                                   : 0;
                    }

                    if (this.Count < 1)
                    {
                        return this.TotalSeconds;
                    }

                    return Math.Round(new TimeSpan(this._lngTotalTicks / this.Count).TotalSeconds, 3);
                }
            }

            /// <summary>
            ///     Gets the minimum running time of this performance tracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public double MinSeconds
            {
                get
                {
                    if (Disabled)
                    {
                        return -1;
                    }

                    if (this._ptrParent == null)
                    {
                        return this._colDetail != null && this._colDetail.Count > 0 ? Math.Round(this._colDetail.Min(subTracker => subTracker.Value.MinSeconds), 3) : 0;
                    }

                    if (this.Count < 1)
                    {
                        return 0;
                    }

                    return Math.Round(new TimeSpan(this._lngMinTicks).TotalSeconds, 3);
                }
            }

            /// <summary>
            ///     Gets the maximum running time of this performance tracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public double MaxSeconds
            {
                get
                {
                    if (Disabled)
                    {
                        return -1;
                    }

                    if (this._ptrParent == null)
                    {
                        return this._colDetail != null && this._colDetail.Count > 0 ? Math.Round(this._colDetail.Max(subTracker => subTracker.Value.MaxSeconds), 3) : 0;
                    }

                    return Math.Round(new TimeSpan(this._lngMaxTicks).TotalSeconds, 3);
                }
            }

            /// <summary>
            ///     Gets the total time that is not accounted for in this PerfTracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public double MissingSeconds
            {
                get
                {
                    if (this._ptrParent == null || (this._colDetail != null && this._colDetail.Count < 1))
                    {
                        return 0;
                    }

                    // Get the total time of all the children
                    long dblDetailTotalTicks = 0;
                    foreach (PerfTracker ptr in this._colDetail.Values)
                    {
                        dblDetailTotalTicks += ptr._lngTotalTicks;
                    }

                    // Return the difference
                    return Math.Round(new TimeSpan(this._lngTotalTicks - dblDetailTotalTicks).TotalSeconds, 3);
                }
            }

            /// <summary>
            ///     Gets the number of missing StopStack calls.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public int MissingStopCalls
            {
                get
                {
                    if (this._ptrParent == null)
                    {
                        return 0;
                    }

                    return this._intStartCount - this._intStopCount;
                }
            }

            /// <summary>
            ///     Gets the total seconds for all child objects on this PerfTracker object.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public double TotalChildrenSeconds => Math.Round(new TimeSpan(this.TotalChildrenTicks).TotalSeconds, 3);

            /// <summary>
            ///     Gets the total ticks for all child objects on this PerfTracker object.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public long TotalChildrenTicks
            {
                get
                {
                    // Get the total time of all the children
                    long lngDetailTotalTicks = 0;
                    if (this.Children != null)
                    {
                        foreach (PerfTracker ptr in this.Children.Values)
                        {
                            lngDetailTotalTicks += ptr._lngTotalTicks;
                        }
                    }

                    return lngDetailTotalTicks;
                }
            }

            /// <summary>
            /// Injects a new perftracker with the specified counts and times into the current path.
            /// </summary>
            /// <param name="insertName">The name of the PerfTracker to inject.</param>
            /// <param name="insertTotalSeconds">The total time (in seconds) for the PerfTracker to inject.</param>
            public void Insert(string insertName, double insertTotalSeconds)
            {
                if (!Disabled && this._ptrCurrent != null)
                {
                    try
                    {
                        PerfTracker st = null;
                        lock (this._objMyLock)
                        {
                            if (this._ptrCurrent._colDetail.ContainsKey(insertName))
                            {
                                // Already have this subtracker in here, just update it
                                st = this._ptrCurrent._colDetail[insertName];
                            }
                            else
                            {
                                // Need a new subtracker
                                st = new PerfTracker(insertName);
                                this._ptrCurrent._colDetail.TryAdd(insertName, st);
                                st._ptrParent = this._ptrCurrent;
                            }

                            // Mark this injected PerfTracker as started
                            this._ptrCurrent = st;

                            // Now update this PerfTracker's count and time
                            long ticks = (long)Math.Floor(insertTotalSeconds * TimeSpan.TicksPerSecond);
                            this._ptrCurrent._lngTotalTicks += ticks;
                            this._ptrCurrent._intCount++;
                            this._ptrCurrent._intStartCount++;

                            // Mark this PerfTracker as stopped
                            this._ptrCurrent = this._ptrCurrent._ptrParent;
                            this._ptrCurrent._intStopCount++;

                            // Update statistics
                            if (ticks < this._lngMinTicks)
                            {
                                this._lngMinTicks = ticks;
                            }

                            if (ticks > this._lngMaxTicks)
                            {
                                this._lngMaxTicks = ticks;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }
            }

            /// <summary>
            ///     Gets the ordered list of Performance Trackers (for output).
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            public PerfTracker[] Detail
            {
                get
                {
                    return this.Children != null ? this.Children.OrderBy(kvp => kvp.Value.CreationTime).Select(kvp => kvp.Value).ToArray() : Enumerable.Empty<PerfTracker>().ToArray();
                }
            }

            /// <summary>
            ///     Gets or sets the child performance trackers of this performance tracker.
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            private ConcurrentDictionary<string, PerfTracker> Children
            {
                get
                {
                    if (Disabled)
                    {
                        return default(ConcurrentDictionary<string, PerfTracker>);
                    }

                    if (this._colDetail != null && this._colDetail.Count < 1)
                    {
                        return default(ConcurrentDictionary<string, PerfTracker>);
                    }

                    return this._colDetail;
                }

                set
                {
                    this._colDetail = value;
                }
            }

            /// <summary>
            /// Returns a string representing this PerfTracker's current stack
            /// ([Start]/[Child]/[Child]/[Child]).
            /// </summary>
            public string CurrentPath(bool isRoot = false)
            {
                if (isRoot)
                {
                    if (this._ptrCurrent._ptrParent == null)
                    {
                        //No parent - Return the current PerfTracker's name
                        return this._ptrCurrent.Name;
                    }
                    else
                    {
                        //We have a parent - Return a string representing the current PerfTracker's [Parent]/[Current]
                        return this._ptrCurrent._ptrParent.CurrentPath(false) + "/" + this._ptrCurrent.Name;
                    }
                }
                else
                {
                    if (this._ptrParent == null)
                    {
                        //No parent - Return this PerfTracker's name
                        return this.Name;
                    }
                    else
                    {
                        //We have a parent - Return a string representing this PerfTracker's [Parent]/[Current]
                        return this._ptrParent.CurrentPath(false) + "/" + this.Name;
                    }
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Initializes a new instance of the <see cref="PerfTracker"/> class.
            ///     Creates a new performance tracker with the specified name.
            /// </summary>
            /// <param name="n">
            /// </param>
            public PerfTracker(string n)
            {
                try
                {
                    this.Name = n;
                    this._ptrCurrent = this;
                    this._lngStart = DateTime.Now.Ticks;
                    this.CreationTime = DateTime.Now;
                }
                catch (Exception)
                {
                    // Failed - Do nothing
                }
            }

            /// <summary>
            /// Adds the specified PerfTracker to this PerfTracker's children.
            /// </summary>
            /// <param name="ptrIn">
            /// The PerfTracker to be added.
            /// </param>
            public void Add(PerfTracker ptrIn)
            {
                try
                {
                    lock (this._objMyLock)
                    {
                        if (this._ptrCurrent.Children == null)
                        {
                            this._ptrCurrent._colDetail = new ConcurrentDictionary<string, PerfTracker>();
                            this._ptrCurrent._colDetail.TryAdd(ptrIn.Name, ptrIn);
                            return;
                        }

                        this._ptrCurrent.Children.TryAdd(ptrIn.Name, ptrIn);
                    }
                }
                catch (Exception)
                {
                    // Failed - Do nothing
                }
            }

            /// <summary>
            /// Inserts a note with 0 time into this PerfTracker in the current tracker.
            /// </summary>
            /// <param name="note">
            /// </param>
            public void AddNote(string note)
            {
                this.StartStack("NOTE: " + note);
                this.StopStack("NOTE: " + note);
            }

            /// <summary>
            ///     Clears this Performance Tracker and all its children.
            /// </summary>
            public void Reset()
            {
                try
                {
                    if (!Disabled && this.Children != null)
                    {
                        lock (this._objMyLock)
                        {
                            foreach (PerfTracker st in this.Children.Values)
                            {
                                st.Reset();
                            }

                            this.Children.Clear();
                        }
                    }
                }
                catch (Exception)
                {
                    // Failed - Do nothing
                }
            }

            /// <summary>
            /// Start tracking the specified step.
            /// </summary>
            /// <param name="n">
            /// The step name.
            /// </param>
            public void StartStack(string n)
            {
                if (!Disabled && this._ptrCurrent != null)
                {
                    try
                    {
                        PerfTracker st = null;
                        lock (this._objMyLock)
                        {
                            if (this._ptrCurrent._colDetail.ContainsKey(n))
                            {
                                // Already have this subtracker in here, just update it
                                st = this._ptrCurrent._colDetail[n];
                            }
                            else
                            {
                                // Need a new subtracker
                                st = new PerfTracker(n);
                                this._ptrCurrent._colDetail.TryAdd(n, st);
                                st._ptrParent = this._ptrCurrent;
                            }

                            this._ptrCurrent = st;
                            this._ptrCurrent.StartTimer();
                        }
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }
            }

            /// <summary>
            /// Stop tracking the specified step.
            /// </summary>
            /// <param name="n">
            /// The step name.
            /// </param>
            public void StopStack(string n)
            {
                try
                {
                    if (!Disabled && this._ptrCurrent != null)
                    {
                        bool blnFound = false;
                        if (this._ptrCurrent.Name == n || string.IsNullOrEmpty(n))
                        {
                            // Found the subtracker (or assume current), stop it
                            if (string.IsNullOrEmpty(n))
                            {
                                this._ptrCurrent._intEmptyStopCalls++;
                            }
                            else
                            {
                                blnFound = true;
                            }

                            this._ptrCurrent.StopTimer(blnFound);
                            this._ptrCurrent = this._ptrCurrent._ptrParent;
                        }
                        else
                        {
                            // Might be a missing StopStack somewhere, check the parents for the specified name
                            PerfTracker t = this._ptrCurrent;
                            while (t != null && t.Name != n)
                            {
                                // Still not found, move up to the parent
                                t = t._ptrParent;
                            }

                            if (t != null)
                            {
                                if (t.Name == n)
                                {
                                    // Found it!
                                    this._ptrCurrent = t;
                                    this._ptrCurrent._intMissingStopCalls++;
                                    blnFound = true;
                                }
                                else
                                {
                                    // Not found, assume current
                                    this._ptrCurrent._intMismatchStopCalls++;
                                }
                            }
                            else
                            {
                                // Not found, assume current
                                this._ptrCurrent._intMismatchStopCalls++;
                            }

                            if (this._ptrCurrent != null)
                            {
                                this._ptrCurrent.StopTimer(blnFound);
                                this._ptrCurrent = this._ptrCurrent._ptrParent;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }

            /// <summary>
            /// Returns a formatted string representing this performance tracker.
            /// </summary>
            /// <param name="intOffset">
            /// The indent offset to use for this tree.
            /// </param>
            /// <param name="decMinTimeToShow">
            /// The minimum time (in seconds) to show in this tree.
            /// </param>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public string SummaryTree(int intOffset = 0, double decMinTimeToShow = 0)
            {
                try
                {
                    if (!Disabled)
                    {
                        StringBuilder stbOut = new StringBuilder();
                        StringBuilder stbName = new StringBuilder();
                        string warning = string.Empty;
                        string pre = string.Empty;

                        if (this.TotalSeconds >= decMinTimeToShow || this.TotalChildrenSeconds >= decMinTimeToShow || this.Name.StartsWith("NOTE: "))
                        {
                            for (int i = 0; i <= intOffset - 1; i++)
                            {
                                pre += "| ";
                            }

                            // Format the name (so all the times for items on the same level are lined up)
                            stbName.Append(this.Name);
                            if (this.Name.Length < 30)
                            {
                                stbName.Append(' ', 30 - this.Name.Length);
                            }

                            if (this.MissingStopCalls != 0)
                            {
                                // Mismatch in the number of explicit Start() vs Stop() calls
                                warning = " (!! Missing " + this.MissingStopCalls + " stop call" + (this.MissingStopCalls > 1 ? "s" : string.Empty) + " !!)";
                            }

                            // Build the output line
                            if (this._ptrParent == null)
                            {
                                // This is the master perftracker
                                stbOut.AppendFormat("{0}+- {1}{2}", pre, stbName, "\r\n");
                            }
                            else
                            {
                                if (this.Count < 2)
                                {
                                    // Only one occurrence, spit it out without average/min/max
                                    stbOut.AppendFormat(
                                        "{0}+- {1}           {2} s {3}{4}",
                                        pre,
                                        stbName,
                                        this.TotalSeconds.ToString("0.000"),
                                        warning,
                                        "\r\n");
                                }
                                else
                                {
                                    // More than one occurrence, spit it out with average/min/max
                                    stbOut.AppendFormat(
                                        "{0}+- {1}           {2} s ({3} @ {4} - m: {5} - M: {6}){7}{8}",
                                        pre,
                                        stbName,
                                        this.TotalSeconds.ToString("0.000"),
                                        this.Count,
                                        this.AvgSeconds.ToString("0.000"),
                                        this.MinSeconds.ToString("0.000"),
                                        this.MaxSeconds.ToString("0.000"),
                                        warning,
                                        "\r\n");
                                }
                            }

                            // Build the output for the children
                            lock (this._objMyLock)
                            {
                                if (this.Detail != null)
                                {
                                    foreach (PerfTracker st in this.Detail)
                                    {
                                        stbOut.Append(st.SummaryTree(intOffset + 1, decMinTimeToShow));
                                    }
                                }
                            }

                            // Build the output for the uncounted seconds
                            if (this.MissingSeconds >= decMinTimeToShow)
                            {
                                // We have missing seconds, show it as "Other"
                                stbOut.AppendFormat(
                                    "{0}+- {1}                        {2} s {3}{4}",
                                    "| " + pre,
                                    "[Missing Seconds]",
                                    this.MissingSeconds.ToString("0.000"),
                                    string.Empty,
                                    "\r\n");
                            }
                        }

                        // Done
                        return stbOut.ToString();
                    }

                    // Performance tracking is globally disabled, return nothing
                    return "DISABLED";
                }
                catch (Exception ex)
                {
                    // There was an error
                    return "PerfTracker.SummaryTree() Failed: " + "\r\n" + ex;
                }
            }

            /// <summary>
            ///     Stops all the child performance trackers of this performance tracker.
            /// </summary>
            private void StopChildren()
            {
                try
                {
                    if (!Disabled && this.Children != null)
                    {
                        // Stop all my children
                        lock (this._objMyLock)
                        {
                            foreach (PerfTracker st in this.Children.Values)
                            {
                                if (st != null)
                                {
                                    if (st._blnIsRunning)
                                    {
                                        st.StopTimer(false);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Failed - Do nothing
                }
            }

            /// <summary>
            ///     Start the step.
            /// </summary>
            private void StartTimer()
            {
                try
                {
                    if (!Disabled)
                    {
                        this._intStartCount++;
                        this._blnIsRunning = true;
                        this._lngStart = DateTime.Now.Ticks;
                    }
                }
                catch (Exception)
                {
                    // Failed - Do nothing
                }
            }

            /// <summary>
            /// Stop the step.
            /// </summary>
            /// <param name="blnExplicit">
            /// Indicates whether this Stop was called with the correct, explicit PerfTracker name. This is used to compare number of Start() calls to number of Stop() calls.
            /// </param>
            private void StopTimer(bool blnExplicit)
            {
                try
                {
                    if (!Disabled)
                    {
                        if (blnExplicit)
                        {
                            this._intStopCount++;
                        }

                        this._blnIsRunning = false;
                        this._intCount++;
                        this.StopChildren();
                        long lngTicks = DateTime.Now.Ticks - this._lngStart;
                        this._lngTotalTicks += lngTicks;
                        if (lngTicks < this._lngMinTicks)
                        {
                            this._lngMinTicks = lngTicks;
                        }

                        if (lngTicks > this._lngMaxTicks)
                        {
                            this._lngMaxTicks = lngTicks;
                        }
                    }
                }
                catch (Exception)
                {
                    // Failed - Do nothing
                }
            }

            #endregion
        }
    }
}
