using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;                                                  // a

public static class MapInfo {
    static public int W { get; private set; }                          // b
    static public int H { get; private set; }
    static public int[,] MAP { get; private set; }
    static public Vector3 OFFSET = new Vector3( 0.5f, 0.5f, 0 );             // c

    /// <summary>
    /// Load map data from the delverLevel TextAsset (e.g., DelverLevel_Eagle)
    /// </summary>
    public static void LoadMap(string mapString) {
        // Read in the map data as an array of lines
        string[] lines = mapString.Split( '\n' );                       // d
        H = lines.Length;
        string[] tileNums = lines[0].Trim().Split( ' ' );// A space between ' '// e
        W = tileNums.Length;

        // Place the map data into a 2D Array for very fast access
        MAP = new int[W, H];             // Generate a 2D array of the right size
        for ( int j = 0; j < H; j++ ) {  // Iterate over every line in lines
            tileNums = lines[j].Trim().Split( ' ' );  // A space between ' '   // f 
            for ( int i = 0; i < W; i++ ) {  // Iterate over every tileNum string
                if ( tileNums[i] == ".." ) {                                 // g
                    MAP[i, j] = 0;
                } else {                                                     // h
                    MAP[i, j] = int.Parse( tileNums[i], NumberStyles.HexNumber );
                }
            }
        }

        Debug.Log( "Map size: " + W + " wide by " + H + " high" );
    }

    /// <summary>
    /// Used by TilemapManager to get the bounds of the map
    /// </summary>
    /// <returns></returns>
    public static BoundsInt GET_MAP_BOUNDS() {                               // i
        BoundsInt bounds = new BoundsInt( 0, 0, 0, W, H, 1 );
        return bounds;
    }

    /// <summary>
    /// Returns the tileNum at specific coordinates.
    /// </summary>
    /// <param name="pos">The position to check as a Vector2</param>
    /// <returns>The tileNum at that location of the MAP</returns>
    public static int GET_MAP_AT_VECTOR2( Vector2 pos ) {                    // c
        Vector2Int posInt = Vector2Int.FloorToInt( pos );
        return MAP[posInt.x, posInt.y];
    }
}
