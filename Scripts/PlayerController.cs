using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : TileItem
{
    private List<AttachableBox> attachedBoxes;
    public List<AttachableBox> AttachedBoxes { get { return attachedBoxes.ToList(); }} // copy of the attached box list
    public HashSet<TileItem> AllPlayerTiles { get { 
        HashSet<TileItem> set = new HashSet<TileItem>(attachedBoxes);
        set.Add(this);
        return set;
    }}

    public PlayerAnimation megAnimation;
    public PlayerAnimation tessAnimation;
    public PlayerAnimation playerAnimation { get; private set; }
    private SpriteRenderer[] faceSprites;

    public float moveTime;

    private enum PlayerState {
        STATIC, // state for being in edit mode
        READY, // state for being able to take inputs and move
        ATTACHING, // state for attaching or detaching
        MOVING, // state for being in motion
        DEAD, // state for not accepting inputs because it has been "destroyed"
        WIN // state for not accepting inputs because the level is over
    }
    PlayerState playerState;

    public enum PlayerMouseAction {
        NONE,
        DETACH,
        ATTACH
    }
    Vector2 previousMousePoint;
    int currentMouseDown;
    PlayerMouseAction currentMouseAction;

    float xInput;
    float yInput;

    Coroutine currentMoveCoroutine;
    public Vector2 currentDirection { get; private set; }

    private Vector2 initialPosition;

    public static EmptyDelegate OnAttach;
    public static EmptyDelegate OnDetach;
    public static EmptyDelegate OnFailAttach;
    public static EmptyDelegate OnDie;
    public static EmptyDelegate OnWin;
    public static EmptyDelegate OnHitWall;

    public delegate void PlayerMoveAction(Vector2 dir);
    public static PlayerMoveAction OnStartMove;
    public static PlayerMoveAction OnStartStep;
    public static PlayerMoveAction OnEndStep;
    public static PlayerMoveAction OnEndMove;
    

    public GameObject burnParticles;
    public GameObject poofParticles;


    public Camera cam { get; private set; }


    private void Awake() {
        playerState = PlayerState.STATIC;

        if (PlayerPrefs.GetInt("character", 0) != 1) {
            // selected meg
            playerAnimation = megAnimation;
            megAnimation.gameObject.SetActive(true);
            tessAnimation.gameObject.SetActive(false);
        }
        else {
            // selected tess
            playerAnimation = tessAnimation;
            megAnimation.gameObject.SetActive(false);
            tessAnimation.gameObject.SetActive(true);
        }

        // set sprite things
        spriteContainer = playerAnimation.spriteContainer;
        sr = playerAnimation.GetComponent<SpriteRenderer>();
        faceSprites = playerAnimation.faceContainer.GetComponentsInChildren<SpriteRenderer>();
    }

    protected override void Start() {
        base.Start();
        attachedBoxes = new List<AttachableBox>();
    }

    public override void StartPlay() {
        playerState = PlayerState.READY;
        initialPosition = tilePosition;

        levelTiles.SetPlayer(this);

        cam = Camera.main;
    }


    private void Update()
    {
        if (isPlaying && playerState == PlayerState.READY || playerState == PlayerState.MOVING) {
            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Vertical");
        }
        else {
            // reset inputs
            xInput = 0;
            yInput = 0;
        }
        
        if (isPlaying && playerState == PlayerState.READY || playerState == PlayerState.ATTACHING) {
            CheckAttachInput();

            // set the player's state based on whether they are currently holding down a mouse button
            playerState = currentMouseDown >= 0 ? PlayerState.ATTACHING : PlayerState.READY;
        }
        else {
            // reset inputs
            currentMouseDown = -1;
            currentMouseAction = PlayerMouseAction.NONE;
        }


        if (playerState == PlayerState.MOVING) {
            SetAllSortingLayers();
        }
    }


    private void FixedUpdate() {
        if (playerState == PlayerState.READY) {
            if (xInput != 0) {
                // this is a left or right input
                TryStartMove(new Vector2(xInput, 0));
            }
            else if (yInput != 0) {
                // this is an up or down input
                TryStartMove(new Vector2(0, yInput));
            }
        }
    }

    // returns whether a move actually started
    private bool TryStartMove(Vector2 dir) {
        currentDirection = dir; // temporarily set current direction for wall check
        if (CheckForWall(dir)) {
            // we can't move this way
            currentDirection = Vector2.zero;
            return false;
        }

        if (currentMoveCoroutine != null) {
            StopCoroutine(currentMoveCoroutine);
        }

        currentMoveCoroutine = StartCoroutine(Move(dir));
        return true;
    }

    // slides the player all the way across
    IEnumerator Move(Vector2 dir) {
        // everything that happens here should be undoable
        levelManager.OpenNewStateFrame("Move " + dir);
        RecordState();

        OnStartMove?.Invoke(dir);

        playerState = PlayerState.MOVING;
        currentDirection = dir;
        
        do {
            yield return StartCoroutine(Step(dir));
        } while (playerState == PlayerState.MOVING && !CheckForWall(dir));

        // round resultant position to nearest 0.5 units
        transform.position = tilePosition;
        SetAllSortingLayers();

        if (playerState == PlayerState.MOVING) {
            // strong overlap check at the end of every uninterrupted move
            levelManager.CheckOverlaps(true);
        }

        // the player could have had a playerState change during the strong overlap check, so there is a duplicate if statement here.
        if (playerState == PlayerState.MOVING) {
            playerState = PlayerState.READY;
        }

        if (playerState == PlayerState.READY) {
            OnHitWall?.Invoke();
        }

        OnEndMove?.Invoke(dir);
        levelManager?.CloseCurrentStateFrame();

        currentDirection = Vector2.zero;
    }

    // takes one step to an adjacent tile
    IEnumerator Step(Vector2 dir) {
        OnStartStep?.Invoke(dir);

        Vector3 initialPosition = tilePosition;
        Vector3 targetPosition = tilePosition + dir;

        for (float t = 0; playerState == PlayerState.MOVING && t < moveTime; t += Time.fixedDeltaTime) {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, t / moveTime);
            yield return new WaitForFixedUpdate();
        }

        if (playerState == PlayerState.MOVING) {
            transform.position = targetPosition;
            SetAllSortingLayers();

            // weak overlap check at the end of every uninterrupted step
            levelManager.CheckOverlaps(false);
        }
        
        // also do anything here
        OnEndStep?.Invoke(dir);
    }


    // check if there is a wall in the desired direction.
    private bool CheckForWall(Vector2 dir, bool doBumpAnimation = true) {
        // check for a wall adjacent to the player or any attached box
        return levelManager.CheckForBlockage(transform.position.ToVector2(), dir, doBumpAnimation)
            | attachedBoxes.Any(box => levelManager.CheckForBlockage(box.transform.position.ToVector2(), dir, doBumpAnimation));

            // above i used single or ("|") intentionally to allow both wall checks to trigger bump animations
    }


    private void SetAllSortingLayers() {
        SetSortingLayer();
        foreach (AttachableBox box in attachedBoxes) {
            box.SetSortingLayer();
        }
    }

    public override void SetSortingLayer() {
        base.SetSortingLayer();
        foreach (SpriteRenderer sprite in faceSprites) {
            if (sprite.name.Contains("glass")) {
                sprite.sortingOrder = sr.sortingOrder + 2;
            } else {
                sprite.sortingOrder = sr.sortingOrder + 1;
            }
        }
    }


    private void CheckAttachInput() {
        Vector2 mousePoint = cam.ScreenToWorldPoint(Input.mousePosition).ToTilePosition();

        if (currentMouseDown >= 0) {
            // there is a mouse button currently down, so handle that first
            if (!Input.GetMouseButton(currentMouseDown)) {
                if (currentMouseAction != PlayerMouseAction.NONE) {
                    levelManager.CloseCurrentStateFrame();
                }

                currentMouseDown = -1;
                currentMouseAction = PlayerMouseAction.NONE;
            }
            else if (mousePoint != previousMousePoint) {
                // the mouse button is still held and has been dragged
                currentMouseAction = HandleAttachInput(mousePoint, currentMouseDown, currentMouseAction);
            }
        }

        if (currentMouseDown == -1) {
            // check for a new mouse input
            for (int i = 0; i <= 1; i++) {
                if (Input.GetMouseButtonDown(i)) {
                    currentMouseDown = i;
                    previousMousePoint = mousePoint;
                    currentMouseAction = HandleAttachInput(mousePoint, i, currentMouseAction);
                }
            }
        }

        previousMousePoint = mousePoint;
    }

    // returns an updated value of currentMouseAction based on what happens here
    private PlayerMouseAction HandleAttachInput(Vector2 mousePoint, int mouseButton, PlayerMouseAction previousMouseAction) {
        // get boxes at the mouse position
        List<AttachableBox> boxes =
            levelManager.GetTilesAt<AttachableBox>(
                cam.ScreenToWorldPoint(Input.mousePosition).ToTilePosition()
            ).ToList();

        if (boxes.Count == 0) {
            // no boxes here, do nothing
            return previousMouseAction;
        }

        // use the first box in the list
        AttachableBox box = boxes[0];
        if (boxes.Count > 1) {
            Debug.LogError("There should not be more than one box at location " + mousePoint + ".");
        }

        bool changed = false;
        bool shouldOpenStateFrame = previousMouseAction == PlayerMouseAction.NONE;

        if (!box.IsAttached) {
            if (previousMouseAction == PlayerMouseAction.DETACH) {
                // currently detaching, so ignore this already detached box
                return previousMouseAction;
            }

            if (mouseButton == 0) {
                // left click means attach one
                changed = TryAttachBox(box, shouldOpenStateFrame);
            }
            else if (mouseButton == 1) {
                // right click means attach all possible
                changed = AttachAllBoxes(shouldOpenStateFrame);
            }

            return changed ? PlayerMouseAction.ATTACH : previousMouseAction;
        }
        else {
            if (previousMouseAction == PlayerMouseAction.ATTACH) {
                // currently attaching, so ignore this already attached box
                return previousMouseAction;
            }

            if (mouseButton == 0) {
                // left click means detach one
                changed = DetachBox(box, shouldOpenStateFrame);
            }
            else if (mouseButton == 1) {
                // right click means detach all
                changed = DetachAllBoxes(shouldOpenStateFrame);
            }

            return changed ? PlayerMouseAction.DETACH : previousMouseAction;
        }
    }

    private bool TryAttachBox(AttachableBox box, bool shouldOpenStateFrame = false) {
        if (box.IsAdjacentToPlayer(this)) {
            if (shouldOpenStateFrame) {
                OpenAttachStateFrame(true);
            }

            // this box can be attached
            box.AttachTo(this);
            attachedBoxes.Add(box);

            OnAttach?.Invoke();
            return true;
        } else {
            // indicate that this box cannot be attached at this time
            box.RejectAttach();
            
            OnFailAttach?.Invoke();
            return false;
        }
    }

    private bool AttachAllBoxes(bool shouldOpenStateFrame = false) {
        HashSet<AttachableBox> boxes = GetAllNeighboringBoxes(true);

        bool didAttach = false;
        foreach (AttachableBox box in boxes) {
            if (!box.IsAttached) {
                if (!didAttach && shouldOpenStateFrame) {
                    OpenAttachStateFrame(true);   
                }

                // attach this box
                box.AttachTo(this);
                attachedBoxes.Add(box);
                didAttach = true;
            }
        }

        if (didAttach) {
            OnAttach?.Invoke();
        }
        return didAttach;
    }

    public bool DetachBox(AttachableBox box, bool shouldOpenStateFrame = false) {
        if (!box.IsAttached) {
            return false;
        }

        if (shouldOpenStateFrame) {
            OpenAttachStateFrame(false);
        }
        
        box.Detach();
        attachedBoxes.Remove(box);

        HashSet<AttachableBox> connected = GetAllNeighboringBoxes(false);

        for (int b = attachedBoxes.Count - 1; b >= 0; b--) {
            if (!connected.Contains(attachedBoxes[b])) {
                attachedBoxes[b].Detach();
                attachedBoxes.RemoveAt(b);
            }
        }

        OnDetach?.Invoke();
        return true;
    }

    public bool DetachAllBoxes(bool shouldOpenStateFrame = false) {
        if (attachedBoxes.Count == 0) {
            return false;
        }

        if (shouldOpenStateFrame) {
            OpenAttachStateFrame(false);
        }

        foreach (AttachableBox box in attachedBoxes) {
            box.Detach();
        }
        attachedBoxes.Clear();

        OnDetach?.Invoke();
        return true;
    }


    // isAttaching = true for attach, isAttaching = false for detach
    private void OpenAttachStateFrame(bool isAttaching) {
        levelManager.OpenNewStateFrame(isAttaching ? "Attach" : "Detach");
        RecordState();
    }


    private AttachableBox GetAttachedBox(Vector2 tilePosition) {
        return attachedBoxes.FirstOrDefault(box => box.transform.position.ToTilePosition() == tilePosition);
    }

    private AttachableBox GetUnattachedBox(Vector2 tilePosition) {
        List<AttachableBox> boxes = levelManager.GetTilesAt<AttachableBox>(tilePosition).ToList();

        if (boxes.Count == 0) {
            // no box here
            return null;
        }

        // multiple boxes on one tile is illegal
        if (boxes.Count > 1) {
            Debug.LogError("There should not be more than one box at location " + tilePosition + ".");
        }

        return boxes[0];
    }

    private HashSet<AttachableBox> GetAllNeighboringBoxes(bool considerUnattached = false) {
        // it's DFS time!
        HashSet<AttachableBox> visited = new HashSet<AttachableBox>();
        Stack<AttachableBox> nodes = new Stack<AttachableBox>();

        foreach (Direction dir in DirectionExt.cardinals) {
            Vector2 adjPos = transform.position.ToTilePosition() + dir.ToVector2();

            AttachableBox adjBox = GetAttachedBox(adjPos);
            if (considerUnattached && adjBox == null) {
                adjBox = GetUnattachedBox(adjPos);
            }

            if (adjBox != null) {
                nodes.Push(adjBox);
            }
        }

        while (nodes.Count > 0) {
            AttachableBox box = nodes.Pop();

            if (visited.Contains(box)) {
                continue;
            }
            visited.Add(box);

            foreach (Direction dir in DirectionExt.cardinals) {
                Vector2 newPos = box.transform.position.ToTilePosition() + dir.ToVector2();

                AttachableBox newBox = GetAttachedBox(newPos);
                if (considerUnattached && newBox == null) {
                    newBox = GetUnattachedBox(newPos);
                }

                if (newBox != null) {
                    nodes.Push(newBox);
                }
            }
        }

        return visited;
    }


    // whether or not a portion of the player's hitbox is at this tile position
    public bool OccupiesTile(Vector2 pos) {
        Vector2 tilePos = pos.ToTilePosition();
        if (tilePos == this.tilePosition) {
            return true;
        }

        return GetAttachedBox(tilePos) != null;
    }


    public void Win() {
        DetachAllBoxes();
        playerState = PlayerState.WIN;
    
        OnWin?.Invoke();
    }


    public override void Die() {
        playerState = PlayerState.DEAD;

        // detach all boxes before "destroying" self
        DetachAllBoxes();

        base.Die();
        OnDie?.Invoke();
    }

    public void DieByFire() {
        Instantiate(burnParticles, transform.position, Quaternion.identity);
        Die();
    }


    public override TileState GetTileState() {
        return new PlayerTileState(tilePosition, levelTiles.IsDynamic(this), playerState, attachedBoxes, megAnimation.faceContainer.transform.localScale);
    }

    public override void RevertState(TileState state) {
        if (state is PlayerTileState tileState) {
            // handle dynamic first
            if (tileState.isDynamic && !levelTiles.IsDynamic(this)) {
                levelTiles.SetDynamic(this);
            } else if (!tileState.isDynamic && levelTiles.IsDynamic(this)) {
                levelTiles.SetStatic(this);
            }

            base.RevertState(state);
            SetAllSortingLayers();

            // handle player state
            playerState = tileState.state;

            // handle attachable boxes
            attachedBoxes = tileState.boxes;

            // handle face
            megAnimation.faceContainer.transform.localScale = tileState.faceScale;
        }
        else {
            Debug.LogError("Cannot revert PlayerController using non-player state " + state + "!");
        }
    }

    public override void Reset(bool undoable) {
        if (undoable) {
            RecordState();
        }

        if (levelTiles.IsDynamic(this)) {
            // the player is dynamic right now, so they may have boxes attached. remove those
            DetachAllBoxes();
        } else {
            // player should be dynamic to start
            levelTiles.SetDynamic(this);
        }

        playerState = PlayerState.READY;

        // since the player is a dynamic tile, we can move its position directly 
        transform.position = initialPosition.ToVector3();
        SetSortingLayer();

        // handle face
        megAnimation.faceContainer.transform.localScale = Vector3.one;
    }


    public override void StopPlay() {
        DetachAllBoxes();

        // since the player is a dynamic tile, we can move its position directly 
        transform.position = initialPosition.ToVector3();
        SetSortingLayer();

        // handle face
        megAnimation.faceContainer.transform.localScale = Vector3.one;

        // set the player to static
        levelTiles.SetStatic(this);
        playerState = PlayerState.STATIC;
    
        levelManager = null;
    }



    private class PlayerTileState : TileState {
        public bool isDynamic { get; private set; }
        public PlayerState state { get; private set; }
        public List<AttachableBox> boxes { get; private set; }
        public Vector3 faceScale { get; private set; }

        public PlayerTileState(Vector2 position, bool isDynamic,
            PlayerState state, List<AttachableBox> boxes, Vector3 faceScale) : base(position)
        {
            this.isDynamic = isDynamic;
            this.state = state;
            this.boxes = new List<AttachableBox>(boxes);
            this.faceScale = faceScale;
        }
    }
}
