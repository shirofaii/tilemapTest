using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SmartWallTile : Tile {

    [Flags]
    private enum Side {
        None = 0,
        T    = 1,
        TR   = 2,
        R    = 4,
        BR   = 8,
        B    = 16,
        BL   = 32,
        L    = 64,
        TL   = 128,
        All  = 255
    }

    public Sprite[] sprites;

    public override void RefreshTile(Vector3Int location, ITilemap tilemap) {
        for (var yd = -1; yd <= 1; yd++) {
            for (int xd = -1; xd <= 1; xd++) {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if(HasWall(tilemap, position)) {
                    tilemap.RefreshTile(position);
                }
            }
        }
    }

    // This determines which sprite is used based on the walls that are adjacent to it
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData) {
        byte mask = 0;

        if(HasWall(tilemap, location + new Vector3Int( 0,  1, 0))) mask |= (byte)Side.T;
        if(HasWall(tilemap, location + new Vector3Int( 1,  1, 0))) mask |= (byte)Side.TR;
        if(HasWall(tilemap, location + new Vector3Int( 1,  0, 0))) mask |= (byte)Side.R;
        if(HasWall(tilemap, location + new Vector3Int( 1, -1, 0))) mask |= (byte)Side.BR;
        if(HasWall(tilemap, location + new Vector3Int( 0, -1, 0))) mask |= (byte)Side.B;
        if(HasWall(tilemap, location + new Vector3Int(-1, -1, 0))) mask |= (byte)Side.BL;
        if(HasWall(tilemap, location + new Vector3Int(-1,  0, 0))) mask |= (byte)Side.L;
        if(HasWall(tilemap, location + new Vector3Int(-1,  1, 0))) mask |= (byte)Side.TL;
        
        tileData.sprite       = GetSpriteByMask(mask);
    }

    private bool HasWall(ITilemap tilemap, Vector3Int position) {
        return tilemap.GetTile(position) == this;
    }

    // The following determines which sprite to use based on the number of adjacent RoadTiles
    private Sprite GetSpriteByMask(byte mask) {
        return sprites[mask];
        // if(mask == (byte)(Side.All)) return sprites[0];

        // var axis = mask & (byte)(Side.T | Side.R | Side.B | Side.L);
        // switch (axis) {
        //     case (byte)Side.None:         return sprites[1];
        //     case (byte)Side.T:            return sprites[2];
        //     case (byte)Side.R:            return sprites[3];
        //     case (byte)Side.B:            return sprites[4];
        //     case (byte)Side.L:            return sprites[5];
        //     case (byte)(Side.T | Side.B): return sprites[6];
        //     case (byte)(Side.R | Side.L): return sprites[7];
        //     case (byte)(Side.T | Side.R): {
        //         if(mask == (byte)(Side.T | Side.R | Side.TR)) return sprites[8];
        //         return sprites[9];
        //     }
        //     case (byte)(Side.R | Side.B): {
        //         if(mask == (byte)(Side.B | Side.R | Side.BR)) return sprites[10];
        //         return sprites[11];
        //     }
        //     case (byte)(Side.B | Side.L): {
        //         if(mask == (byte)(Side.B | Side.L | Side.BL)) return sprites[12];
        //         return sprites[13];
        //     }
        //     case (byte)(Side.L | Side.T): {
        //         if(mask == (byte)(Side.T | Side.L | Side.TL)) return sprites[14];
        //         return sprites[15];
        //     }
        //     case (byte)(Side.T | Side.B | Side.L): {
        //         switch(mask) {
        //             case (byte)(Side.T | Side.B | Side.L | Side.TL): return sprites[16];
        //             case (byte)(Side.T | Side.B | Side.L | Side.BL): return sprites[17];
        //             case (byte)(Side.T | Side.B | Side.L | Side.TL | Side.BL): return sprites[18];
        //             default: return sprites[19];
        //         }
        //     }
        //     case (byte)(Side.T | Side.B | Side.R): {
        //         switch(mask) {
        //             case (byte)(Side.T | Side.B | Side.R | Side.TR): return sprites[20];
        //             case (byte)(Side.T | Side.B | Side.R | Side.BR): return sprites[21];
        //             case (byte)(Side.T | Side.B | Side.R | Side.TR | Side.BR): return sprites[22];
        //             default: return sprites[23];
        //         }
        //     }
        //     case (byte)(Side.R | Side.L | Side.T): {
        //         switch(mask) {
        //             case (byte)(Side.R | Side.L | Side.T | Side.TR): return sprites[24];
        //             case (byte)(Side.R | Side.L | Side.T | Side.TL): return sprites[25];
        //             case (byte)(Side.R | Side.L | Side.T | Side.TR | Side.TL): return sprites[26];
        //             default: return sprites[27];
        //         }
        //     }
        //     case (byte)(Side.R | Side.L | Side.B): {
        //         switch(mask) {
        //             case (byte)(Side.R | Side.L | Side.B | Side.BR): return sprites[28];
        //             case (byte)(Side.R | Side.L | Side.B | Side.BL): return sprites[29];
        //             case (byte)(Side.R | Side.L | Side.B | Side.BR | Side.BL): return sprites[30];
        //             default: return sprites[31];
        //         }
        //     }
        //     case (byte)(Side.R | Side.L | Side.B | Side.T): {
        //         const Side cross = Side.R | Side.L | Side.B | Side.T;
        //         switch(mask) {
        //             case (byte)(cross | Side.TR): return sprites[32];
        //             case (byte)(cross | Side.BR): return sprites[33];
        //             case (byte)(cross | Side.BL): return sprites[34];
        //             case (byte)(cross | Side.TL): return sprites[35];

        //             case (byte)(cross | Side.TR | Side.BR): return sprites[36];
        //             case (byte)(cross | Side.TR | Side.BL): return sprites[37];
        //             case (byte)(cross | Side.TR | Side.TL): return sprites[38];
        //             case (byte)(cross | Side.BR | Side.BL): return sprites[39];
        //             case (byte)(cross | Side.BR | Side.TL): return sprites[40];
        //             case (byte)(cross | Side.BL | Side.TL): return sprites[41];

        //             case (byte)(cross | Side.TR | Side.BR | Side.TL): return sprites[42];
        //             case (byte)(cross | Side.BR | Side.TR | Side.BL): return sprites[43];
        //             case (byte)(cross | Side.BL | Side.BR | Side.TL): return sprites[44];
        //             case (byte)(cross | Side.TL | Side.TR | Side.BL): return sprites[45];

        //             default: return sprites[46];
        //         }
        //     }
        //     default: throw new Exception();
        // }
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/SmartWallTile")]
    public static void CreateRoadTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save SmartWallTile", "SmartWallTile", "asset", "Save SmartWallTile", "Assets");
        if (path == "") return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SmartWallTile>(), path);
    }
#endif

}


