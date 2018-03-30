using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WallTile : Tile {

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
    private Dictionary<Side, Sprite> generatedByBitmap = new Dictionary<Side, Sprite>();
    private Texture2D atlas;

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
        //generatedByMask = new Sprite[256];
#if UNITY_EDITOR
        if(generatedByMask[mask] == null) {
            GenerateTile((Side)mask);
        }
#endif
        return generatedByMask[mask];
    }

#if UNITY_EDITOR
    private void GenerateTile(Side mask) {
        var bitmap = GetTextureBitmapByMask(mask);
        if(!generatedByBitmap.ContainsKey(bitmap)) {
            var w = (int)filledTile.rect.width;
            var tex = new Texture2D(w, w);

            tex.SetPixels(filledTile.GetPixels());
            GenerateTile(tex, bitmap, w/3);
            tex.Apply();

            generatedByBitmap[bitmap] = Sprite.Create(tex, new Rect(0, 0, w, w), new Vector2(0.5f, 0.5f));
        }
        generatedByMask[(byte)mask] = generatedByBitmap[bitmap];
    }
    
    // generate tile texture from smaller tiles (tile = 3x3 smaller tiles)
    private void GenerateTile(Texture2D tex, Side bitmap, int w) {
        if(!bitmap.HasFlag(Side.T))  tex.SetPixels(w, w*2, w, w, GetSmallTile(bitmap, Side.T).GetPixels());
        if(!bitmap.HasFlag(Side.TR)) tex.SetPixels(w*2, w*2, w, w, GetSmallTile(bitmap, Side.TR).GetPixels());
        if(!bitmap.HasFlag(Side.R))  tex.SetPixels(w*2, w, w, w, GetSmallTile(bitmap, Side.R).GetPixels());
        if(!bitmap.HasFlag(Side.BR)) tex.SetPixels(w*2, 0, w, w, GetSmallTile(bitmap, Side.BR).GetPixels());
        if(!bitmap.HasFlag(Side.B))  tex.SetPixels(w, 0, w, w, GetSmallTile(bitmap, Side.B).GetPixels());
        if(!bitmap.HasFlag(Side.BL)) tex.SetPixels(0, 0, w, w, GetSmallTile(bitmap, Side.BL).GetPixels());
        if(!bitmap.HasFlag(Side.L))  tex.SetPixels(0, w, w, w, GetSmallTile(bitmap, Side.L).GetPixels());
        if(!bitmap.HasFlag(Side.TL)) tex.SetPixels(0, w*2, w, w, GetSmallTile(bitmap, Side.TL).GetPixels());
    }

    private Sprite GetSmallTile(Side bitmap, Side side) {
        var t = bitmap.HasFlag(Side.T);
        var r = bitmap.HasFlag(Side.R);
        var b = bitmap.HasFlag(Side.B);
        var l = bitmap.HasFlag(Side.L);
        switch(side) {
            case Side.T: return edgeTileParts[0];
            case Side.TR:
                if(!t && r) return holeTileParts[4];
                if(t && r)  return holeTileParts[5];
                if(t && !r) return holeTileParts[6];
                return edgeTileParts[1];
            case Side.R: return edgeTileParts[2];
            case Side.BR:
                if(!b && r) return holeTileParts[0];
                if(b && r)  return holeTileParts[7];
                if(b && !r) return holeTileParts[6];
                return edgeTileParts[3];
            case Side.B: return edgeTileParts[4];
            case Side.BL:
                if(!b && l) return holeTileParts[0];
                if(b && l)  return holeTileParts[1];
                if(b && !l) return holeTileParts[2];
                return edgeTileParts[5];
            case Side.L: return edgeTileParts[6];
            case Side.TL:
                if(!t && l) return holeTileParts[4];
                if(t && l)  return holeTileParts[3];
                if(t && !l) return holeTileParts[2];
                return edgeTileParts[7];
            default: throw new Exception();
        }
    }

    private Side GetTextureBitmapByMask(Side mask) {
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
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WallTile>(), path);
    }

    // [ContextMenu("Generate Atlas")]
    // public void GenerateAtlas() {
    //     if(atlas == null) atlas = new Texture2D((int)filledTile.rect.width * 8, (int)filledTile.rect.height * 8);
    //     var textures = generatedByBitmap.Values.Select(x => x.texture).ToArray();
    //     var rects = atlas.PackTextures(textures, 4);
    //     for(var i = 0; i < rects.Length; i++) {
    //         var sprite = Sprite.Create(atlas, rects[i], new Vector2(0.5f, 0.5f));
    //         generatedByBitmap.Where(x => x.Value == textures[i]).Select(x => generatedByBitmap[x.Key] = sprite)
    //     }
    // }
#endif

}

public static class ExtSprite {
    public static Color[] GetPixels(this Sprite sprite) {
        var r = sprite.rect;
        return sprite.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
    }
}