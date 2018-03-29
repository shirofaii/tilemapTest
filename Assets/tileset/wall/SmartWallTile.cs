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

    public Sprite filledTile;
    public Sprite[] edgeTileParts;
    public Sprite[] holeTileParts;

    private Sprite[] generatedByMask = new Sprite[256];
    private Dictionary<Side, Sprite> generatedByPattern = new Dictionary<Side, Sprite>();

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

    private Sprite GetSpriteByMask(byte mask) {
        generatedByMask = new Sprite[256];
#if UNITY_EDITOR
        if(generatedByMask[mask] == null) {
            GenerateTile((Side)mask);
        }
#endif
        return generatedByMask[mask];
    }

#if UNITY_EDITOR
    private void GenerateTile(Side mask) {
        var pattern = GetTexturePatternByMask(mask);
        //if(!generatedByPattern.ContainsKey(pattern)) {
            var ft = filledTile;
            var w = (int)ft.rect.width;
            var tex = new Texture2D(w, w);

            tex.SetPixels(ft.texture.GetPixels((int)ft.rect.x, (int)ft.rect.y, (int)ft.rect.width, (int)ft.rect.height));
            tex.Apply();

            generatedByPattern[pattern] = Sprite.Create(tex, new Rect(0, 0, w, w), new Vector2(0.5f, 0.5f));
        //}
        generatedByMask[(byte)mask] = generatedByPattern[pattern];
    }

    private Side GetTexturePatternByMask(Side mask) {
        if(mask == Side.All) return mask;

        var axis = mask & (Side.T | Side.R | Side.B | Side.L);
        switch (axis) {
            case Side.None:
            case Side.T:
            case Side.R:
            case Side.B:
            case Side.L:
            case Side.T | Side.B:
            case Side.R | Side.L: return axis;

            case Side.T | Side.R: return axis | (mask & Side.TR);
            case Side.R | Side.B: return axis | (mask & Side.BR);
            case Side.B | Side.L: return axis | (mask & Side.BL);
            case Side.L | Side.T: return axis | (mask & Side.TL);

            case Side.T | Side.B | Side.L: return axis | (mask & (Side.TL | Side.BL));
            case Side.T | Side.B | Side.R: return axis | (mask & (Side.TR | Side.BR));
            case Side.R | Side.L | Side.T: return axis | (mask & (Side.TR | Side.TL));
            case Side.R | Side.L | Side.B: return axis | (mask & (Side.BR | Side.BL));

            case Side.R | Side.L | Side.B | Side.T: return mask;

            default: throw new Exception();
        }
    }

    [MenuItem("Assets/Create/SmartWallTile")]
    public static void CreateRoadTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save SmartWallTile", "SmartWallTile", "asset", "Save SmartWallTile", "Assets");
        if (path == "") return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SmartWallTile>(), path);
    }
#endif

}

