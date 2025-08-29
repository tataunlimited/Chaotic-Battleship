using System.Collections;
using Core.Board;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private static WaitForSeconds _waitForSeconds1 = new(1f);
    public BoardController boardController;
    public CameraController cameraController;
    public enum PHASE_STATE
    {
        START_ENCOUNTER, ENEMY_PLACING_SHIPS, PLAYER_PLACING_SHIPS, PLAYER_FIRING, ENEMY_FIRING, ENEMY_MOVING, PLAYER_MOVING, ENDWAVE
    }
    public PHASE_STATE phaseState;
    public bool playerShipsPlaced;
    public bool enemyShipsPlaced;
    public bool winConditionMet;
    public bool loseConditionMet;


    public GameObject GameOverPanel;
    public GameObject NextWavePanel;
    public TextMeshProUGUI WaveCountText;


    public static GameManager Get()
    {
        return GameObject.Find("OBJ_GameManager").GetComponent<GameManager>();
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        phaseState = PHASE_STATE.START_ENCOUNTER;
        playerShipsPlaced = false;
        enemyShipsPlaced = false;
        winConditionMet = false;
        loseConditionMet = false;
        WaveCountText.text = PlayerData.Instance.waveNumber.ToString();
        cameraController = Camera.main.GetComponent<CameraController>();
        StartEncounter();
        instance = this;
    }

    public void Restart()
    {
        Init();
        boardController.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextPhaseButton();
        }
    }

    private bool CanMoveToNextPhase()
    {
        return AreAllShipsPlaced();
    }

    private bool AreAllShipsPlaced()
    {
        return BoardController.Instance.playerView.AllShipsArePlaced;
    }

    public void NextPhaseButton()
    {
        if(!CanMoveToNextPhase())
            return;
        BoardController.Instance.ClearUI();

        switch (phaseState)
        {
            // case PHASE_STATE.START_ENCOUNTER:
            //     // Jason: After GameOver.Restart() reloads the scene, StartEncounterCoroutine only runs to the yield
            //     //      so was hoping StopCoroutine would reset it, but no luck
            //     //StopCoroutine(StartEncounterCoroutine(1f));     
            //     //StartCoroutine(StartEncounterCoroutine(1f));
            //     StartEncounter();
            //     break;

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
                phaseState = PHASE_STATE.ENEMY_MOVING;
                Debug.Log("Phase changed to: ENEMY_MOVING");
                break;

            case PHASE_STATE.ENEMY_MOVING:
                phaseState = PHASE_STATE.PLAYER_MOVING;
                Debug.Log("Phase changed to: PLAYER_MOVING");
                break;

            case PHASE_STATE.PLAYER_MOVING:

                boardController.ResetGridIndicators();
                Debug.Log("Player Movement Confirmed");
                phaseState = PHASE_STATE.PLAYER_FIRING;
                Debug.Log("Phase changed to: PLAYER_FIRING");
                StartCoroutine(AttackingPhase());
                break;

            case PHASE_STATE.ENDWAVE:
                // Reset or prepare for the next encounter
                EndWave();
                phaseState = PHASE_STATE.START_ENCOUNTER;
                Debug.Log("Phase reset to: START_ENCOUNTER");
                break;
        }

    }

    /*private IEnumerator StartEncounterCoroutine(float wait_time)
    {
        phaseState = PHASE_STATE.ENEMY_PLACING_SHIPS;
        if (!enemyShipsPlaced)
        { 
            boardController.SpawnEnemyShips();
            enemyShipsPlaced = true;
        }
        yield return new WaitForSeconds(wait_time);
        
        phaseState = PHASE_STATE.PLAYER_PLACING_SHIPS;
        if (!playerShipsPlaced)
        {
            PlacePlayerShips();
            playerShipsPlaced = true;
        }
        else
            boardController.playerView.SaveShipLocations();     // saves all of the ships locations/rotations in case reset button is pressed
    }*/
    private void StartEncounter()
    {
        // Initialize wave-specific parameters
        Debug.Log("Starting new encounter...");
        phaseState = PHASE_STATE.ENEMY_PLACING_SHIPS;
        Debug.Log("Phase changed to: ENEMY_PLACING_SHIPS");
        if (!enemyShipsPlaced)
        {
            Debug.Log("Placing enemy ships...");
            boardController.SpawnEnemyShips();
            enemyShipsPlaced = true;
        }

        phaseState = PHASE_STATE.PLAYER_PLACING_SHIPS;
        Debug.Log("Phase changed to: PLAYER_PLACING_SHIPS");
        if (!playerShipsPlaced)
        {
            Debug.Log("Placing player ships...");
            PlacePlayerShips();
            playerShipsPlaced = true;
        }
        else
            boardController.playerView.SaveShipLocations();     // saves all of the ships locations/rotations in case reset button is pressed
    }


    // Logic for placing player ships
    private void PlacePlayerShips()
    {
        //TODO: activate player ship placement from UI onto player grid

        Debug.Log("Waiting for player to place ships...");

        //boardController.SpawnPlayerShips();      TODO: this is temporary until we implement proper ship placement
        boardController.playerView.SaveShipLocations(); // TODO: this is temporary just for testing resetting. it shouldn't be needed since ships can be placed anywhere at will
    }

    private void BeginBattle()
    {
        // Logic to start the battle

        //TODO: deactivate player ship placement from UI onto player grid

        Debug.Log("Player Ship placement confirmed");
        Debug.Log("Starting Battle...");
        boardController.UpdateBoards();
        cameraController.GoToAttackView();
        phaseState = PHASE_STATE.PLAYER_FIRING;
        Debug.Log("Phase changed to: PLAYER_FIRING");
        StartCoroutine(AttackingPhase());
    }

    private IEnumerator AttackingPhase()
    {
        // Logic for player firing

        //TODO: deactivate / unhighlight player movement options
        cameraController.GoToAttackView();
        yield return _waitForSeconds1;
        boardController.playerView.ClearPhaseFX();
        boardController.enemyView.ClearPhaseFX();
        boardController.PlayerAttack();
        Debug.Log("Player Fired!");
        
        yield return _waitForSeconds1;
        phaseState = PHASE_STATE.ENEMY_FIRING;
        boardController.EnemyAttack();
        
        yield return _waitForSeconds1;
        yield return _waitForSeconds1;

       
        CheckEndWaveConditions();
    }

    private void EnemyFires()
    {
        // Logic for enemy firing

        //TODO: activate Fire function on all enemy ships, resolve hits

        //TODO: set Wave end conditions if met

        Debug.Log("Enemy Fired!");

    }

    private void CheckEndWaveConditions()
    {
        // Logic to check if the wave has ended
        Debug.Log("Checking end wave conditions...");
        winConditionMet = boardController.enemyView.AllShipsAreDestroyed();
        loseConditionMet = boardController.playerView.AllShipsAreDestroyed();

        if (winConditionMet)
        {
            Debug.Log("Wave end conditions met, player wins!");
            boardController.enemyView.RevealShips();    // show enemy board with destroyed ships

            phaseState = PHASE_STATE.ENDWAVE;
            Debug.Log("Phase changed to: ENDWAVE");
        }
        else if (loseConditionMet)
        {
            Debug.Log("Wave end conditions met, player loses!");
            boardController.enemyView.RevealShips();    // show enemy board with destroyed ships

            phaseState = PHASE_STATE.ENDWAVE;
            Debug.Log("Phase changed to: ENDWAVE");
        }
        else
        {
            Debug.Log("Wave end conditions not met, continuing...");
            phaseState = PHASE_STATE.ENEMY_MOVING;
            Debug.Log("Phase changed to: ENEMY_MOVING");
            EnemyMoves();
            cameraController.GoToDefaultView();
        }

    }

    private void EnemyMoves()
    {
        // Logic for enemy movement

        //move all enemy ships according to AI rules
        boardController.UpdateEnemyShips();

        Debug.Log("Enemy is moving...");
        phaseState = PHASE_STATE.PLAYER_MOVING;
        Debug.Log("Phase changed to: PLAYER_MOVING");
        PlayerMoves();
    }

    // Logic for player movement
    private void PlayerMoves()
    {
        boardController.playerView.SaveShipLocations();     // saves all of the ships locations/rotations in case reset button is pressed

        Debug.Log("Waiting for Player to move...");


        //TODO: Activate allowed movement (allow player to move/rotate ships around the grid)
    }

    private void EndWave()
    {
        // Logic for ending the wave
        Debug.Log("Ending wave...");

        if (winConditionMet)
        {
            NextWavePanel.SetActive(true);
        }
        else
        {
            GameOverPanel.SetActive(true);
        }
    }

    public void StartNextWave()
    {
        // remove all enemy ship instances
        boardController.enemyView.Reset();
        enemyShipsPlaced = false;

        // cleanup player board
        boardController.playerView.ResetIndicators();
        boardController.ClearSelectedShip();
        boardController.playerView.HealAllShips();

        PlayerData.Instance.waveNumber ++; // Increment wave number
        WaveCountText.text = PlayerData.Instance.waveNumber.ToString();
        winConditionMet = false; // Reset win condition
        phaseState = PHASE_STATE.START_ENCOUNTER;
    }

}