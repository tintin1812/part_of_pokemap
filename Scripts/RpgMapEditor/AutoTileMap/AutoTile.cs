using UnityEngine;
using System.Collections;
using System;


namespace AON.RpgMapEditor
{

    // public enum eLayerType
    // {
    //     Ground = 0, // tiles with predefined collisions
    //     Overlay = 1, // tiles no collision
    //     Trigger = 2 // objects like triggers and actors
    // };
    

    // [Obsolete("This has been deprecated after adding multiple layer support!")]
    /// <summary>
    /// Each type of tile layer in the map
    /// </summary>
    // public enum eTileLayer
    // {
    //     /// <summary>
    //     /// mostly for tiles with no alpha
    //     /// </summary>
    //     GROUND,
    //     /// <summary>
    //     /// mostly for tiles with alpha
    //     /// </summary>
    //     GROUND_OVERLAY,
    //     /// <summary>
    //     /// for tiles that should be drawn over everything else
    //     /// </summary>
    //     OVERLAY
    // }

    /// <summary>
    /// Each type of tile of the map
    /// </summary>
    // public enum eTileType
    // {
    //     /// <summary>
    //     /// Animated auto-tiles with 3 frames of animation, usually named with _A1 suffix in the texture
    //     /// </summary>
    //     ANIMATED,
    //     /// <summary>
    //     /// Ground auto-Tiles, usually named with _A2 suffix in the texture
    //     /// </summary>
    //     GROUND,
    //     /// <summary>
    //     /// Building auto-Tiles, usually named with _A3 suffix in the texture
    //     /// </summary>
    //     BUILDINGS,
    //     /// <summary>
    //     /// Wall auto-Tiles, usually named with _A4 suffix in the texture
    //     /// </summary>
    //     WALLS,
    //     /// <summary>
    //     /// Normal tiles, usually named with _A5 suffix in the texture. Same as Objects tiles, but included as part of an auto-tileset
    //     /// </summary>
    //     NORMAL,
    //     /// <summary>
    //     /// Normal tiles, usually named with _B, _C, _D and _E suffix in the texture
    //     /// </summary>
    //     OBJECTS
    // };

    /// <summary>
    /// Type map collision according to tile on certain map position
    /// </summary>
    public enum eTileCollisionType
    {
        /// <summary>
        /// Used to indicate the empty tile with no type
        /// </summary>
        EMPTY = -1,
        /// <summary>
        /// A PASSABLE tile over a BLOC, WALL, or FENCE allow walking over it.
        /// </summary>
        PASSABLE,
        /// <summary>
        /// Not passable
        /// </summary>
        BLOCK,
        /// <summary>
        /// Partially not passable, depending of autotiling
        /// </summary>
        WALL,
        /// <summary>
        /// Partially not passable, depending of autotiling
        /// </summary>
        FENCE,
        /// <summary>
        /// A passable tile. Used to check when a tile should be placed in overlay layer
        /// </summary>
        OVERLAY,
        /// <summary>
        /// The size of this enum
        /// </summary>
        _SIZE
    }

    /// <summary>
    /// Define a tile of the map
    /// </summary>        
    public class AutoTile
    {
        public int Id = -1;
        
        public int TileX;
        
        public int TileY;
        
        public int Layer;
    }
}
