using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LevelTiles : MonoBehaviour {

    [SerializeField] private TileDictionary tileDictionary;
    public TileDictionary TileDictionary { get { return tileDictionary; }}

    [SerializeField] private ColorDictionary colorDictionary;
    public ColorDictionary ColorDictionary { get { return colorDictionary; }}

    public SpriteRenderer dropShadowRight;
    public SpriteRenderer dropShadowDown;
    public ParticleSystem backgroundParticles;


    private bool loaded;



    /**************************/
    /******* PROPERTIES *******/
    /**************************/
    public string levelName;
    public string levelAuthor;
    public string description;

    public ColorData[] colorDatas { get; private set; }

    public Color GetColor(string name) {
        ColorData colorData = colorDatas.FirstOrDefault(data => data.name.ToLower() == name.ToLower());
        
        if (colorData.name?.ToLower() != name.ToLower()) {
            Debug.LogError("There is no color in LevelTiles named " + name + "!");
            return Color.clear;
        }
        else {
            return colorData.Color;
        }
    }

    public LevelFlags levelFlags;


    /**************************/
    /********* BOUNDS *********/
    /**************************/
    [SerializeField] private SpriteRenderer floor;
    public Rect Rect { get {
        return new Rect(Mathf.RoundToInt(floor.bounds.min.x), Mathf.RoundToInt(floor.bounds.min.y), Width, Height);
    }}
    public int Width  { get { return Mathf.RoundToInt(floor.size.x); }}
    public int Height { get { return Mathf.RoundToInt(floor.size.y); }}

    [SerializeField] private Vector2Int maxSize;
    [SerializeField] private Vector2Int minSize;
    public int MaxWidth { get { return maxSize.x; }}
    public int MaxHeight { get { return maxSize.y; }}
    public int MinWidth { get { return minSize.x; }}
    public int MinHeight { get { return minSize.y; }}


    /**************************/
    /********* TILES **********/
    /**************************/
    public Transform tileParent;

    public HashSet<TileItem> allTiles { get; private set; }
    public HashSet<TileItem> dynamicTiles { get; private set; }
    public Dictionary<Vector2, HashSet<TileItem>> staticTilesDict { get; private set; }

    private HashSet<TileItem> outerWalls;
    [SerializeField] private GameObject wallPrefab;

    public PlayerController player { get; private set; }


    private void Awake() {
        if (!loaded) {
            LoadExistingTiles();
            RebuildWalls();
        }
    }


    /**************************/
    /******* GENERATION *******/
    /**************************/
    private void Init() {
        allTiles = new HashSet<TileItem>();
        dynamicTiles = new HashSet<TileItem>();
        staticTilesDict = new Dictionary<Vector2, HashSet<TileItem>>();
        outerWalls = new HashSet<TileItem>();

        if (colorDatas == null) {
            ResetColors();
        }
    }

    private void LoadExistingTiles() {
        Init();    

        foreach (Transform tile in tileParent) {
            TileItem tileItem = tile.GetComponent<TileItem>();
            if (tileItem == null) {
                // this is not a tile. ignore it
                continue;
            }

            AddTile(tileItem);
            tileItem.levelTiles = this;
        }

        OnChangedSize();

        loaded = true;
    }

    public void LoadFromLevelInfo(LevelInfo levelInfo) {
        if (levelInfo == null) {
            return;
        }

        if (loaded) {
            // first, let's destroy the existing map
            foreach (TileItem tile in allTiles) {
                Destroy(tile.gameObject);
            }
        }

        Init();

        levelName = levelInfo.levelName;
        levelAuthor = levelInfo.levelAuthor;
        description = levelInfo.description;

        levelFlags = levelInfo.flags;

        for (int c = 0; c < levelInfo.colorHues.Length; c++) {
            colorDatas[c].hue = levelInfo.colorHues[c];
        }

        SetSize(levelInfo.width, levelInfo.height);
        RebuildWalls();

        foreach (TileInfoData tileData in levelInfo.tiles) {
            Vector2 position = GridToTilePosition(tileData.x, tileData.y);
            CreateTile(tileDictionary.tileTypes[tileData.i].prefab, position, tileData.a);
        }

        loaded = true;
    }

    private void ResetColors() {
        // this clones the color dictionary so that we don't modify it at runtime
        colorDatas = colorDictionary.colorDatas.Where(_ => true).ToArray();
    }

    private void SetSize(byte width, byte height) {
        floor.size = new Vector2(width, height);
        floor.transform.position += new Vector3(0.25f, 0.25f, 0); // this line ensures that the size of the grid doesn't change when snapping
        floor.GetComponent<SnapToGrid>().Snap();

        OnChangedSize();
    }

    public LevelInfo GenerateLevelInfo() {
        LevelInfo info = new LevelInfo();

        info.levelName = levelName;
        info.levelAuthor = levelAuthor;
        info.description = description;

        info.flags = levelFlags;

        info.width = (byte)Width;
        info.height = (byte)Height;

        info.colorHues = colorDatas.Select(data => data.hue).ToArray();

        info.tiles = new List<TileInfoData>();
        foreach (TileItem tile in allTiles) {
            if (outerWalls.Contains(tile)) {
                continue;
            }
            
            info.tiles.Add(tile.GetData());
        }

        return info;
    }

    private void RebuildWalls() {
        // first, destroy the existing outer walls
        if (outerWalls != null) {
            foreach (TileItem wall in outerWalls) {
                DestroyTile(wall);
            }
        }

        outerWalls = new HashSet<TileItem>();

        // place vertical walls
        for (float y = floor.bounds.min.y - 0.5f; y <= floor.bounds.max.y + 0.5f; y += 1f) {
            TileItem leftWall = CreateTile(wallPrefab, new Vector2(floor.bounds.min.x - 0.5f, y), 0, true);
            outerWalls.Add(leftWall);
            
            TileItem rightWall = CreateTile(wallPrefab, new Vector2(floor.bounds.max.x + 0.5f, y), 0, true);
            outerWalls.Add(rightWall);
        }

        // place horizontal walls
        for (float x = floor.bounds.min.x + 0.5f; x <= floor.bounds.max.x - 0.5f; x += 1f) {
            TileItem bottomWall = CreateTile(wallPrefab, new Vector2(x, floor.bounds.min.y - 0.5f), 0, true);
            outerWalls.Add(bottomWall);

            TileItem topWall = CreateTile(wallPrefab, new Vector2(x, floor.bounds.max.y + 0.5f), 0, true);
            outerWalls.Add(topWall);
        }
    }

    public List<TileInfoData> Resize(Direction direction, bool expand) {
        List<TileInfoData> deletions = new List<TileInfoData>();
        
        // handle non lateral directions
        if (direction == Direction.NONE) {
            return deletions;
        }
        if (direction.IsDiagonal()) {
            Resize(direction.GetHorizontal(), expand);
            Resize(direction.GetVertical(), expand);
            return deletions;
        }
        
        // get all of the walls on the level border that is being changed
        List<Wall> adj;
        List<Wall> border = GetLevelBorder(direction, out adj);

        // delete any existing blocks that we need to remove first
        if (!expand) {
            // destroy existing corners
            adj.ForEach(wall => DestroyTile(wall));

            // destroy existing items on top of the new wall position
            border.ForEach(wall => deletions.AddRange(DestroyTilesAt(GetTilePosition(wall) - direction.ToVector2(), null, true)));
        }

        // rescale floor
        int d = expand ? 1 : -1;
        floor.transform.position += d * direction.ToVector3() / 2;
        
        if (direction.IsHorizontal()) {
            // change horizontal size
            floor.size += d * new Vector2(1, 0);
        } else {
            // changer vertical size
            floor.size += d * new Vector2(0, 1);
        }

        // push border in the desired direction
        border.ForEach(wall => MoveStaticTile(wall, GetTilePosition(wall) + d * direction.ToVector2()));

        // create any new blocks that we need to make
        if (expand) {
            // create new corners
            adj.ForEach(wall => CreateTile(wallPrefab, GetTilePosition(wall) + direction.ToVector2(), 0, true));
        }

        OnChangedSize();

        return deletions;
    }


    private void OnChangedSize() {
        dropShadowRight.transform.position = floor.bounds.max + new Vector3(1, 1.25f, 0);
        dropShadowDown.transform.position = floor.bounds.min - new Vector3(1, 1, 0);

        if (backgroundParticles != null) {
            var shape = backgroundParticles.shape;
            shape.scale = new Vector3(floor.bounds.size.x + 6, floor.bounds.size.y + 6, 1);
        }
    }


    /**************************/
    /********* EDITS **********/
    /**************************/
    public TileItem CreateTile(GameObject tileAsset, Vector2 clickPoint, byte alt = (byte)0, bool bypassBoundsCheck = false) {
        clickPoint = clickPoint.ToTilePosition();
        
        // check if it's in bounds
        if (!bypassBoundsCheck && !IsInBounds(clickPoint)) {
            return null;
        }

        // overwrite existing tiles
        DestroyTilesAt(clickPoint);

        // place a tileAsset at the point.
        GameObject newTile = GameObject.Instantiate(tileAsset, clickPoint, Quaternion.identity, tileParent);
        newTile.GetComponent<SnapToGrid>().Snap();

        TileItem tileItem = newTile.GetComponent<TileItem>();
        AddTile(tileItem);
        tileItem.levelTiles = this;
        
        if (!tileItem.SetAltCode(alt)) {
            Debug.Log("TileItem " + tileItem + " was created with an invalid alt code of " + alt + ".");
        }

        return tileItem;
    }

    public TileItem CreateTile(TileInfoData tileInfo) {
        return CreateTile(tileDictionary.tileTypes[tileInfo.i].prefab, GridToTilePosition(tileInfo.x, tileInfo.y), tileInfo.a);
    }

    public List<TileInfoData> DestroyTilesAt(Vector2 clickPoint) {
        return DestroyTilesAt(clickPoint, null, false);
    }

    private List<TileInfoData> DestroyTilesAt(Vector2 clickPoint, TileItem exception, bool bypassBoundsCheck) {
        List<TileInfoData> destroyed = new List<TileInfoData>();
        clickPoint = clickPoint.ToTilePosition();

        // check if it's in bounds
        if (!bypassBoundsCheck && !IsInBounds(clickPoint)) {
            return destroyed;
        }

        // delete existing tiles at this location
        foreach (TileItem tile in GetTilesAt(clickPoint)) {
            if (exception == null || tile != exception) {
                destroyed.Add(tile.GetData());
                DestroyTile(tile);
            }
        }

        return destroyed;
    }

    public void DestroyTile(TileInfoData infoData) {
        TileItem desiredTile = GetTile(infoData);
        if (desiredTile != null) {
            DestroyTile(desiredTile);
        }
    }

    public void DestroyTile(TileItem tile) {
        RemoveTile(tile);
        Destroy(tile.gameObject);
    }


    private bool AddTile(TileItem tileItem) {
        if (tileItem == null || allTiles.Contains(tileItem)) {
            return false;
        }
        
        allTiles.Add(tileItem);
        SetStatic(tileItem);

        return true;
    }

    private bool RemoveTile(TileItem tileItem) {
        if (tileItem == null || !allTiles.Contains(tileItem)) {
            return false;
        }

        SetDynamic(tileItem);
        dynamicTiles.Remove(tileItem);
        
        allTiles.Remove(tileItem);
        return true;
    }

    public bool RemoveFromPlay(TileItem tileItem) {
        if (tileItem == null || !allTiles.Contains(tileItem)) {
            return false;
        }

        SetStatic(tileItem);
        MoveStaticTile(tileItem, GridToTilePosition(new Vector2Int(-100, -100)));
        return true;
    }


    public void SetPlayer(PlayerController player) {
        this.player = player;
        if (!SetDynamic(player)) {
            Debug.LogError("Failed to set player tile " + player + " to dynamic when calling SetPlayer");
        }
    }

    public bool SetStatic(TileItem tileItem) {
        if (tileItem is PlayerController player) {
            Debug.Log("Setting player to static");
        }

        if (tileItem == null || !allTiles.Contains(tileItem)) {
            return false;
        }

        // stores whether tileItem was marked as dynamic before calling this method
        bool wasDynamic = false;

        // remove this tile from the dynamic set, if it is there
        if (dynamicTiles.Contains(tileItem)) {
            dynamicTiles.Remove(tileItem);
            wasDynamic = true;
        }

        Vector2 tilePosition = GetTilePosition(tileItem);

        if (staticTilesDict.ContainsKey(tilePosition) && staticTilesDict[tilePosition].Contains(tileItem)) {
            // this tile is already static.
            if (wasDynamic) {
                Debug.LogError("Tile " + tileItem + " was simultaneously marked as static and dynamic when SetStatic was called. This should never happen.");
            }
            return false;
        }

        // add the tile to the right static set
        if (!staticTilesDict.ContainsKey(tilePosition)) {
            staticTilesDict.Add(tilePosition, new HashSet<TileItem>());
        }
        staticTilesDict[tilePosition].Add(tileItem);
        
        return true;
    }

    public bool SetDynamic(TileItem tileItem) {
        if (tileItem is PlayerController player) {
            Debug.Log("Setting player to dynamic");
        }

        if (tileItem == null || !allTiles.Contains(tileItem)) {
            return false;
        }

        // stores whether tileItem was marked as static before calling this method
        bool wasStatic = false;

        // remove this tile from the static set, if it is there
        Vector2 tilePosition = GetTilePosition(tileItem);

        if (staticTilesDict.ContainsKey(tilePosition) && staticTilesDict[tilePosition].Contains(tileItem)) {
            staticTilesDict[tilePosition].Remove(tileItem);
            wasStatic = true;
        } else {
            // it's possible that this object exists in the static set at a different location.
            // if it is, then that is a bug. since this else block wouldn't run in correct circumstances, let's run the check here.
            foreach (Vector2 pos in staticTilesDict.Keys) {
                if (staticTilesDict[pos].Contains(tileItem)) {
                    Debug.LogError("Static tile " + tileItem + " was not logged at the correct position when SetDynamic was called. This should never happen.");
                    
                    // for now, assume that everything is fine and remove the static tile like normal
                    staticTilesDict[pos].Remove(tileItem);
                    wasStatic = true;
                }
            }
        }

        if (dynamicTiles.Contains(tileItem)) {
            // this tile is already dynamic.
            if (wasStatic) {
                Debug.LogError("Tile " + tileItem + " was simultaneously marked as static and dynamic when SetDynamic was called. This should never happen.");
            }
            return false;
        }

        dynamicTiles.Add(tileItem);
        return true;
    }


    public bool MoveStaticTile(TileItem tileItem, Vector2 newPos) {
        Vector2 oldPos = GetTilePosition(tileItem);
        newPos = newPos.ToTilePosition();

        if (tileItem == null || !staticTilesDict.ContainsKey(oldPos) || !staticTilesDict[oldPos].Contains(tileItem)) {
            return false;
        }

        tileItem.transform.position = newPos;
        tileItem.SetSortingLayer();

        if (!staticTilesDict.ContainsKey(newPos)) {
            staticTilesDict.Add(newPos, new HashSet<TileItem>());
        }
        staticTilesDict[oldPos].Remove(tileItem);
        staticTilesDict[newPos].Add(tileItem);

        return true;
    }


    public void ClearTiles() {
        // destroy everything, then reform the walls
        foreach (TileItem tile in allTiles) {
            Destroy(tile.gameObject);
        }

        Init();
        RebuildWalls();
    }


    public void ClearEverything() {
        // destroy everything
        foreach (TileItem tile in allTiles) {
            Destroy(tile.gameObject);
        }

        Init();
        ResetColors();

        // reset size
        SetSize((byte)10, (byte)10);
        RebuildWalls();

        levelName = "";
        levelAuthor = "";
        description = "";
        levelFlags = new LevelFlags();
    }



    /**************************/
    /******* POSITIONS ********/
    /**************************/
    public static Vector2 GetTilePosition(TileItem item) {
        if (item == null) {
            return new Vector2(0.5f, 0.5f);
        }

        return item.transform.position.ToTilePosition();
    }

    public Vector2Int GetGridPosition(TileItem item) {
        return TileToGridPosition(GetTilePosition(item));
    }

    public Vector2Int TileToGridPosition(Vector2 tilePos) {
        Vector2 bottomLeft = (floor.bounds.min.ToVector2() + new Vector2(0.5f, 0.5f)).ToTilePosition();
        return new Vector2Int(Mathf.RoundToInt(tilePos.x - bottomLeft.x), Mathf.RoundToInt(tilePos.y - bottomLeft.y));
    }

    public Vector2 GridToTilePosition(Vector2Int gridPos) {
        Vector2 bottomLeft = (floor.bounds.min.ToVector2() + new Vector2(0.5f, 0.5f)).ToTilePosition();
        return (bottomLeft + new Vector2(gridPos.x, gridPos.y)).ToTilePosition();
    }

    public Vector2 GridToTilePosition(byte x, byte y) {
        return GridToTilePosition(new Vector2Int(x, y));
    }


    public bool IsInBounds(Vector3 pos) {
        return floor.bounds.Contains(pos);
    }



    /**************************/
    /******** GETTERS *********/
    /**************************/
    public TileItem GetTile(TileInfoData infoData) {
        return GetTilesAt(GridToTilePosition(infoData.x, infoData.y))
            .FirstOrDefault(tile => tile.id == infoData.i && tile.GetAltCode() == infoData.a);
    }

    public HashSet<T> GetTiles<T>() where T : TileItem {
        HashSet<T> result = new HashSet<T>();
        
        foreach (TileItem tile in allTiles) {
            if (tile is T tileT) {
                result.Add(tileT);
            }
        }

        return result;
    }

    public HashSet<T> GetTilesAt<T>(Vector2 pos) where T : TileItem {
        HashSet<T> result = new HashSet<T>();
        
        foreach (TileItem tile in GetTilesAt(pos)) {
            if (tile is T tileT) {
                result.Add(tileT);
            }
        }

        return result;
    }

    public HashSet<TileItem> GetTilesAt(Vector2 pos) {
        Vector2 tilePosition = pos.ToTilePosition();

        HashSet<TileItem> result = new HashSet<TileItem>();

        // add all static tiles to the set
        if (staticTilesDict.ContainsKey(tilePosition)) {
            result = new HashSet<TileItem>(staticTilesDict[tilePosition]);
        }

        // add all dynamic tiles to the set
        foreach (TileItem dynamicTile in dynamicTiles) {
            if (GetTilePosition(dynamicTile) == pos) {
                result.Add(dynamicTile);
            }
        }

        return result;
    }


    public HashSet<TileItem> GetWalls(Vector2 pos) {
        HashSet<TileItem> walls = new HashSet<TileItem>();
        
        foreach (TileItem tile in GetTilesAt(pos)) {
            if (tile.Collider != null && tile.Collider.enabled
                && tile.gameObject.layer == LayerMask.NameToLayer("Wall")
                && !player.AllPlayerTiles.Contains(tile)
            ) {
                walls.Add(tile);
            }
        }

        return walls;
    }

    public TileItem GetTileWithTagAt(string tag, Vector2 pos) {
        return GetTilesAt(pos).FirstOrDefault(tile => tile.CompareTag(tag));
    }

    public bool IsDynamic(TileItem tile) {
        return dynamicTiles.Contains(tile);
    }


    private List<Wall> GetLevelBorder(Direction direction) {
        // adj will be ignored
        List<Wall> adj;
        return GetLevelBorder(direction, out adj);
    }

    private List<Wall> GetLevelBorder(Direction direction, out List<Wall> adj) {
        List<Wall> border = new List<Wall>();
        adj = new List<Wall>();

        switch (direction) {
            case Direction.LEFT:
                // fetch left wall
                for (Vector2Int gridPos = new Vector2Int(-1, -1); gridPos.y <= Height; gridPos.y++) {
                    border.AddRange(GetTilesAt<Wall>(GridToTilePosition(gridPos)));
                }
                
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(0, -1))));
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(0, Height))));
                break;
            case Direction.RIGHT:
                // fetch right wall
                for (Vector2Int gridPos = new Vector2Int(Width, -1); gridPos.y <= Height; gridPos.y++) {
                    border.AddRange(GetTilesAt<Wall>(GridToTilePosition(gridPos)));
                }
                
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(Width - 1, -1))));
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(Width - 1, Height))));
                break;
            case Direction.DOWN:
                // fetch bottom wall
                for (Vector2Int gridPos = new Vector2Int(-1, -1); gridPos.x <= Width; gridPos.x++) {
                    border.AddRange(GetTilesAt<Wall>(GridToTilePosition(gridPos)));
                }
                
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(-1, 0))));
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(Width, 0))));
                break;
            case Direction.UP:
                // fetch top wall
                for (Vector2Int gridPos = new Vector2Int(-1, Height); gridPos.x <= Width; gridPos.x++) {
                    border.AddRange(GetTilesAt<Wall>(GridToTilePosition(gridPos)));
                }
                
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(-1, Height - 1))));
                adj.AddRange(GetTilesAt<Wall>(GridToTilePosition(new Vector2Int(Width, Height - 1))));
                break;
        }

        return border;
    }




    private void Update() {
        floor.color = GetColor("Floor");

        Color bg = GetColor("BG");
        bg = bg.WithVal(bg.GetVal() * 0.8f);
        dropShadowRight.color = bg;
        dropShadowDown.color = bg;
    }
}