using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBrowser.Controls
{
    /// <summary>
    /// stacklayoutextender
    /// </summary>
    public enum FloatAlignment
    {
        /// <summary>
        /// Position relative to the Target's Top-Left corner (Default).
        /// Offsets are applied from this point.
        /// </summary>
        TopLeft,

        /// <summary>
        /// Position immediately to the left of the Target.
        /// Offsets are applied from this point (floater's top-right aligned with target's top-left, before offsets).
        /// </summary>
        ToLeftOf,

        /// <summary>
        /// Position immediately to the right of the Target.
        /// Offsets are applied from this point (floater's top-left aligned with target's top-right, before offsets).
        /// </summary>
        ToRightOf,

        // Potential future additions:
        // Above,
        // Below,
        // Center,
        // TopRight, BottomLeft, BottomRight etc.
    }
}
