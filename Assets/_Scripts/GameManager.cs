using System.Collections;
using Core.Board;
using Core.Ship.Upgrade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private static readonly WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);

    public BoardController boardController;
    public CameraController cameraController;
    
    public ArmorUpgradeSO armorUpgradeSO;
    public AttackPatternUpgradeSO attackUpgradeSO;
    public SpecialAttackUpgradeSO specialUpgradeSO;

    public enum PHASE_STATE
    {
        START_ENCOUNTER,
        ENEMY_PLACING_SHIPS,
        PLAYER_PLACING_SHIPS,
        PLAYER_FIRING,
        ENEMY_FIRING,
        PLAYER_MOVING,
        ENEMY_MOVING,
        ENDWAVE
    }

    public PHASE_STATE phaseState;
    public bool playerShipsPlaced;
    public bool enemyShipsPlaced;
    public bool winConditionMet;
    public bool loseConditionMet;
    public Button nextPhaseBtn;
    public Button rotateLeftButton;
    public Button rotateRightButton;


    public GameObject GameOverPanel;
    public GameObject NextWavePanel;
    public TextMeshProUGUI WaveCountText;

    private ShipPlacementUI _shipPlacementUI;

    public TMP_Text phaseText;
    public TMP_Text roundNumber;
    
    private int _roundNumber = 1;
    public int RoundNumber => _roundNumber;

    // set true when we load a snapshot so we can bypass placement UI gating
    private bool _loadedFromSnapshot = false;

    // ---- Helper to avoid obsolete warnings across Unity versions ----
#if UNITY_2022_2_OR_NEWER
    private static T FindOne<T>() where T : Object => Object.FindFirstObjectByType<T>();
#else
    private static T FindOne<T>() where T : Object => Object.FindObjectOfType<T>();
#endif

    public static GameManager Get()
    {
        var go = GameObject.Find("OBJ_GameManager");
        return go != null ? go.GetComponent<GameManager>() : null;
    }

    private void Start()
    {
        var playerData = PlayerData.Instance;
        playerData.currentPhase = PlayerData.Phase.Placement;
        phaseText.text = playerData.currentPhase.ToString();
        Init();

        roundNumber.text = _roundNumber.ToString();

    }

    private void Init()
    {
        instance = this;

        // Ensure PlayerData is loaded before anything reads wave/score.
        SaveManager.LoadGame();

        phaseState = PHASE_STATE.START_ENCOUNTER;
        playerShipsPlaced = false;
        enemyShipsPlaced = false;
        winConditionMet = false;
        loseConditionMet = false;

        if (WaveCountText != null)
            WaveCountText.text = PlayerData.Instance.waveNumber.ToString();

        if (Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();

        _shipPlacementUI = FindOne<ShipPlacementUI>();
        if (_shipPlacementUI != null)
        {
            _shipPlacementUI.OnAllShipsSpawned += () =>
            {
                if (nextPhaseBtn != null) nextPhaseBtn.interactable = true;
            };
        }

        // ---- Try to load a saved snapshot BEFORE spawning ships ----
        /*
        GameState gs;
        if (SaveManager.TryLoadBoardState(out gs) && gs != null)
        {
            if (gs.waveNumber == PlayerData.Instance.waveNumber)
            {
                // If your BoardController has any lazy init, ensure it's ready
                // before applying a full board snapshot.
                if (boardController != null)
                {
                    // boardController.EnsureEnemyWaveManager(); // uncomment if you have this
                    BoardStateSerializer.Apply(boardController, gs);
                }

                PHASE_STATE parsed;
                if (System.Enum.TryParse(gs.phase, out parsed))
                    phaseState = parsed;

                playerShipsPlaced = true;
                enemyShipsPlaced = true;
                _loadedFromSnapshot = true;

                if (nextPhaseBtn != null) nextPhaseBtn.interactable = true;
                if (cameraController != null) cameraController.GoToDefaultView();

                Debug.Log("[GameManager] Loaded board snapshot.");
                return; // Loaded; skip StartEncounter()
            }
            else
            {
                // Snapshot for different wave; clear so it won't keep trying
                SaveManager.ClearBoardState();
            }
        }
        */

        StartEncounter();
    }

    public void Restart()
    {
        Init();
        if (boardController != null) boardController.Reset();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextPhaseButton();
        }
    }

    private bool CanMoveToNextPhase()
    {
        var bc = (BoardController.Instance != null) ? BoardController.Instance : boardController;

        // During placement phase, if we loaded from a snapshot, ships are already on the board.
        if (phaseState == PHASE_STATE.PLAYER_PLACING_SHIPS)
        {
            bool placed = (bc != null) && bc.playerView.AllShipsArePlaced;
            bool uiReady = _loadedFromSnapshot || (_shipPlacementUI != null && _shipPlacementUI.AreAllShipsSpawned);
            bool btnOK = (nextPhaseBtn == null) || nextPhaseBtn.interactable;
            return placed && uiReady && btnOK;
        }

        // For other phases, just require the "Next" button be interactable (or absent).
        return (nextPhaseBtn == null) || nextPhaseBtn.interactable;
    }

    public void NextPhaseButton()
    {
        if (!CanMoveToNextPhase())
            return;

        if (BoardController.Instance != null)
            BoardController.Instance.ClearUI();

        switch (phaseState)
        {
            case PHASE_STATE.ENEMY_PLACING_SHIPS:
                Debug.Log("Placing enemy ships...");
                break;

            case PHASE_STATE.PLAYER_PLACING_SHIPS:
                BeginBattle();
                break;

            case PHASE_STATE.PLAYER_FIRING:
                phaseState = PHASE_STATE.ENEMY_FIRING;
                Debug.Log("Phase changed to: ENEMY_FIRING");
                break;

            case PHASE_STATE.ENEMY_FIRING:
                phaseState = PHASE_STATE.PLAYER_MOVING;
                Debug.Log("Phase changed to: PLAYER_MOVING");
                break;

            case PHASE_STATE.PLAYER_MOVING:
                if (boardController != null)
                    boardController.ResetGridIndicators();

                phaseState = PHASE_STATE.ENEMY_MOVING;
                Debug.Log("Phase changed to: ENEMY_MOVING");
                EnemyMoves();

                Debug.Log("Player Movement Confirmed");

                // Count a turn when player completes movement
                var scorerB = FindOne<GameManagerScore>();
                if (scorerB != null) scorerB.RegisterPlayerTurn();

                // Snapshot after player commits movement
                SaveSnapshot();

                if (nextPhaseBtn != null) nextPhaseBtn.interactable = false;
                StartCoroutine(AttackingPhase());
                break;

            case PHASE_STATE.ENEMY_MOVING:
                phaseState = PHASE_STATE.PLAYER_FIRING;
                Debug.Log("Phase changed to: PLAYER_FIRING");
                break;

            case PHASE_STATE.ENDWAVE:
                break;
        }
    }

    private void StartEncounter()
    {
        Debug.Log("Starting new encounter...");

        phaseState = PHASE_STATE.ENEMY_PLACING_SHIPS;
        Debug.Log("Phase changed to: ENEMY_PLACING_SHIPS");
        if (!enemyShipsPlaced)
        {
            Debug.Log("Placing enemy ships...");
            if (boardController != null) boardController.SpawnEnemyShips();
            enemyShipsPlaced = true;
        }

        phaseState = PHASE_STATE.PLAYER_PLACING_SHIPS;
        Debug.Log("Phase changed to: PLAYER_PLACING_SHIPS");
        if (!playerShipsPlaced)
        {
            boardController.playerView.Reset();     // removing player's previous ships

            Debug.Log("Placing player ships...");
            PlacePlayerShips();
            playerShipsPlaced = true;
        }
        else
        {
            if (boardController != null)
                boardController.playerView.SaveShipLocations();
        }

        // Snapshot the initial spawned state so stopping Play immediately can resume
        SaveSnapshot();
    }

    private void SaveSnapshot()
    {
        if (boardController == null) return;

        var state = BoardStateSerializer.Capture(
            boardController,
            PlayerData.Instance.waveNumber,
            phaseState.ToString()
        );
        SaveManager.SaveBoardState(state);
    }

    private void PlacePlayerShips()
    {
        Debug.Log("Waiting for player to place ships...");
        // Placement UI shows itself via Update() in ShipPlacementUI during this phase.
        if (boardController != null)
            boardController.playerView.SaveShipLocations();
    }

    private void BeginBattle()
    {
        Debug.Log("Player Ship placement confirmed");
        Debug.Log("Starting Battle...");
        if (boardController != null)
        {
            boardController.UpdateBoards();
            foreach (var ship in boardController.playerView.SpawnedShips)
            {
                ship.Value.SetShipOnGrid(true);
            }
        }
        if (cameraController != null) cameraController.GoToAttackView();

        phaseState = PHASE_STATE.PLAYER_FIRING;
        Debug.Log("Phase changed to: PLAYER_FIRING");
        StartCoroutine(AttackingPhase());
        if (nextPhaseBtn != null) nextPhaseBtn.interactable = false;

        // Snapshot at battle start
        SaveSnapshot();
    }

    private IEnumerator AttackingPhase()
    {
        if (cameraController != null) cameraController.GoToAttackView();
        
        PlayerData.Instance.currentPhase = PlayerData.Phase.Attack;
        phaseText.text = PlayerData.Instance.currentPhase.ToString();
        yield return _waitForSeconds1;

        if (boardController != null)
        {
            boardController.playerView.ClearPhaseFX();
            boardController.enemyView.ClearPhaseFX();
        }
        
        // Player fires
        if (boardController != null)
            yield return StartCoroutine(boardController.PlayerAttack());

        // Count a turn after player attack
        var scorerA = FindOne<GameManagerScore>();
        if (scorerA != null) scorerA.RegisterPlayerTurn();

        // Snapshot after the player's attack resolves
        SaveSnapshot();

        Debug.Log("Player Fired!");

        yield return new WaitForSeconds(.5f);
        Debug.Log("Phase changed to: ENEMY_FIRING");
        phaseState = PHASE_STATE.ENEMY_FIRING;

        // Enemy fires
        if (boardController != null)
            yield return StartCoroutine(boardController.EnemyAttack());

        // Optional: snapshot after enemy attack
        SaveSnapshot();

        yield return new WaitForSeconds(.5f);

        StartCoroutine(CheckEndWaveConditions());
    }

    private IEnumerator CheckEndWaveConditions()
    {
        Debug.Log("Checking end wave conditions...");

        if (boardController != null)
        {
            if (_roundNumber < 10)
            {
                winConditionMet = boardController.enemyView.AllShipsAreDestroyed();
                loseConditionMet = boardController.playerView.AllShipsAreDestroyed();
            }
            else
            {
                var ratio = boardController.enemyView.ComputeTotalHealth()/boardController.playerView.ComputeTotalHealth();
                if (ratio > 1)
                {
                    loseConditionMet = true;
                }
                else
                {
                    winConditionMet = true;
                }
            }
        }

        if (winConditionMet)
        {
            Debug.Log("Wave end conditions met, player wins!");
            if (boardController != null) boardController.enemyView.RevealShips();
            phaseState = PHASE_STATE.ENDWAVE;

            yield return _waitForSeconds1;
            EndWave();
            Debug.Log("Phase changed to: ENDWAVE");
        }
        else if (loseConditionMet)
        {
            Debug.Log("Wave end conditions met, player loses!");
            if (boardController != null) boardController.enemyView.RevealShips();
            phaseState = PHASE_STATE.ENDWAVE;

            yield return _waitForSeconds1;
            EndWave();
            Debug.Log("Phase changed to: ENDWAVE");
        }
        else
        {
            Debug.Log("Wave end conditions not met, continuing...");
            PlayerMoves();

            if (cameraController != null) cameraController.GoToDefaultView();
            if (nextPhaseBtn != null) nextPhaseBtn.interactable = true;
            Debug.Log("nextPhaseBtn.interactable = true");
            _roundNumber++;
            roundNumber.text = _roundNumber.ToString();
        }
    }

    private void EnemyMoves()
    {
        if (boardController != null) boardController.UpdateEnemyShips();
        Debug.Log("Enemy is moving...");

        // Snapshot after enemy movement (optional but useful)
        SaveSnapshot();

        var playerData = PlayerData.Instance;
        playerData.currentPhase = PlayerData.Phase.Movement;
        phaseText.text = playerData.currentPhase.ToString();
    }

    private void PlayerMoves()
    {
        boardController.playerView.SaveShipLocations();     // saves all of the ships locations/rotations in case reset button is pressed
        boardController.playerView.BeginMovementPhase();    // resets their ability to move and rotate

        if (boardController != null) boardController.playerView.SaveShipLocations();
        Debug.Log("Waiting for Player to move...");
    }

    private void EndWave()
    {
        Debug.Log("Ending wave...");

        // We finished the wave (win or lose) → do NOT keep a board snapshot.
        // Clearing here guarantees the next scene load or next wave starts fresh.
        SaveManager.ClearBoardState();

        if (winConditionMet)
        {
            GameEvents.RaiseWaveCleared();   // scoring bonus hook
            if (NextWavePanel != null) NextWavePanel.SetActive(true);

            // Persist meta progress (score/wave/etc.)
            SaveManager.SaveGame();
        }
        else
        {
            if (GameOverPanel != null) GameOverPanel.SetActive(true);

            // Persist meta progress (e.g., final score), but no board.
            SaveManager.SaveGame();
        }
    }

    public void StartNextWave()
    {
        // Extra safety: ensure no stale board snapshot carries over
        SaveManager.ClearBoardState();

        // Enemy cleanup
        if (boardController != null) boardController.enemyView.Reset();
        enemyShipsPlaced = false;

        // Player cleanup
        if (boardController != null)
        {
            boardController.playerView.ResetIndicators();
            boardController.ClearSelectedShip();
            boardController.playerView.HealAllShips();
        }

        // Re-arm the placement UI counts/buttons/gate
        _shipPlacementUI?.ResetForNewWave();

        PlayerData.Instance.waveNumber++;
        if (WaveCountText != null)
            WaveCountText.text = PlayerData.Instance.waveNumber.ToString();

        winConditionMet = false;
        loseConditionMet = false;
        playerShipsPlaced = false;

        phaseState = PHASE_STATE.START_ENCOUNTER;

        // Persist new wave number; we'll snapshot after spawn
        SaveManager.SaveGame();

        // Immediately kick off the new encounter (no scene reload needed)
        StartEncounter();
    }
}
