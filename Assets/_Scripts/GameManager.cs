using System;
using System.Collections;
using Core.Board;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardController boardController;
    public enum PHASE_STATE
    {
        START_ENCOUNTER, ENEMY_PLACING_SHIPS, PLAYER_PLACING_SHIPS, PLAYER_FIRING, ENEMY_FIRING, ENEMY_MOVING, PLAYER_MOVING, ENDWAVE
    }
    public PHASE_STATE phaseState;
    public bool enemyShipsPlaced;
    public bool winConditionMet;
    public bool loseConditionMet;

    public int waveNumber;


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
        enemyShipsPlaced = false;
        winConditionMet = false;
        loseConditionMet = false;
        waveNumber = 0;
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

    public void NextPhaseButton()
    {
        switch (phaseState)
        {
            case PHASE_STATE.START_ENCOUNTER:
                // Jason: After GameOver.Restart() reloads the scene, StartEncounterCoroutine only runs to the yield
                //      so was hoping StopCoroutine would reset it, but no luck
                //StopCoroutine(StartEncounterCoroutine(1f));     
                StartCoroutine(StartEncounterCoroutine(1f));
                break;

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
                phaseState = PHASE_STATE.START_ENCOUNTER;
                enemyShipsPlaced = false; // Reset for next encounter
                Debug.Log("Phase reset to: START_ENCOUNTER");
                break;
        }
    }

    private IEnumerator StartEncounterCoroutine(float wait_time)
    {
        phaseState = PHASE_STATE.ENEMY_PLACING_SHIPS;
        boardController.SpawnEnemyShips();
        yield return new WaitForSeconds(wait_time);
        
        phaseState = PHASE_STATE.PLAYER_PLACING_SHIPS;
        PlacePlayerShips();
    }
    private void StartEncounter()
    {
        // Initialize wave-specific parameters
        Debug.Log("Starting new encounter...");
        phaseState = PHASE_STATE.ENEMY_PLACING_SHIPS;
        Debug.Log("Phase changed to: ENEMY_PLACING_SHIPS");
        Debug.Log("Placing enemy ships...");
        boardController.SpawnEnemyShips();

        phaseState = PHASE_STATE.PLAYER_PLACING_SHIPS;
        Debug.Log("Phase changed to: PLAYER_PLACING_SHIPS");
        PlacePlayerShips();
    }


    // Logic for placing player ships
    private void PlacePlayerShips()
    {
        //TODO: activate player ship placement from UI onto player grid

        Debug.Log("Waiting for player to place ships...");

        boardController.SpawnPlayerShips();     // TODO: this is temporary until we implement proper ship placement
        boardController.playerView.SaveShipLocations(); // TODO: this is temporary just for testing resetting. it shouldn't be needed since ships can be placed anywhere at will
    }

    private void BeginBattle()
    {
        // Logic to start the battle

        //TODO: deactivate player ship placement from UI onto player grid

        Debug.Log("Player Ship placement confirmed");
        Debug.Log("Starting Battle...");
        boardController.UpdateBoards();
        phaseState = PHASE_STATE.PLAYER_FIRING;
        Debug.Log("Phase changed to: PLAYER_FIRING");
        StartCoroutine(AttackingPhase());
    }

    private IEnumerator AttackingPhase()
    {
        // Logic for player firing

        //TODO: deactivate / unhighlight player movement options

        //TODO: activate Fire function on all player ships, resolve hits

        boardController.PlayerAttack();
        Debug.Log("Player Fired!");
        
        yield return new WaitForSeconds(1f);
        phaseState = PHASE_STATE.ENEMY_FIRING;
        boardController.EnemyAttack();
        
        yield return new WaitForSeconds(1f);

       
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
        if (winConditionMet)
        {
            Debug.Log("Wave end conditions met, player wins!");
            phaseState = PHASE_STATE.ENDWAVE;
            Debug.Log("Phase changed to: ENDWAVE");
            EndWave();
        }
        else if (loseConditionMet)
        {
            Debug.Log("Wave end conditions met, player loses!");
            phaseState = PHASE_STATE.ENDWAVE;
            Debug.Log("Phase changed to: ENDWAVE");
            EndWave();
        }
        else
        {
            Debug.Log("Wave end conditions not met, continuing...");
            phaseState = PHASE_STATE.ENEMY_MOVING;
            Debug.Log("Phase changed to: ENEMY_MOVING");
            EnemyMoves();
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
        boardController.playerView.SaveShipLocations();

        Debug.Log("Waiting for Player to move...");


        //TODO: Activate allowed movement (allow player to move/rotate ships around the grid)
    }

    private void EndWave()
    {
        // Logic for ending the wave
        Debug.Log("Ending wave...");
        if (winConditionMet)
        {
            Debug.Log("Starting next wave...");

            waveNumber++; // Increment wave number
            winConditionMet = false; // Reset win condition
            phaseState = PHASE_STATE.START_ENCOUNTER;
        }
        else
        {
            Debug.Log("Restarting from beginning...");

            waveNumber = 0; // Reset wave number
            loseConditionMet = false; // Reset lose condition
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        //TODO: remove all player/enemy ship instances
        Debug.Log("Phase changed to: START_ENCOUNTER");
    }
}