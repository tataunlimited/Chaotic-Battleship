using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum PHASE_STATE
    {
        START_ENCOUNTER, ENEMY_PLACING_SHIPS, PLAYER_PLACING_SHIPS, PLAYER_FIRING, ENEMY_FIRING, ENEMY_MOVING, PLAYER_MOVING, ENDWAVE
    }
    public PHASE_STATE phaseState = PHASE_STATE.START_ENCOUNTER;
    public bool enemyShipsPlaced = false;
    public bool winConditionMet = false;
    public bool loseConditionMet = false;

    public int waveNumber = 0;
    void Start()
    {

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
                StartEncounter();
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

                Debug.Log("Player Movement Confirmed");
                phaseState = PHASE_STATE.PLAYER_FIRING;
                Debug.Log("Phase changed to: PLAYER_FIRING");
                PlayerFires();
                break;

            case PHASE_STATE.ENDWAVE:
                // Reset or prepare for the next encounter
                phaseState = PHASE_STATE.START_ENCOUNTER;
                enemyShipsPlaced = false; // Reset for next encounter
                Debug.Log("Phase reset to: START_ENCOUNTER");
                break;
        }
    }

    private void StartEncounter()
    {
        // Initialize wave-specific parameters
        Debug.Log("Starting new encounter...");
        phaseState = PHASE_STATE.ENEMY_PLACING_SHIPS;
        Debug.Log("Phase changed to: ENEMY_PLACING_SHIPS");
        Debug.Log("Placing enemy ships...");

        //TODO: place enemy ships on enemy grid

        phaseState = PHASE_STATE.PLAYER_PLACING_SHIPS;
        Debug.Log("Phase changed to: PLAYER_PLACING_SHIPS");
        PlacePlayerShips();
    }

    private void PlacePlayerShips()
    {
        // Logic for placing player ships

        //TODO: activate player ship placement from UI onto player grid

        Debug.Log("Waiting for player to place ships...");
    }

    private void BeginBattle()
    {
        // Logic to start the battle

        //TODO: deactivate player ship placement from UI onto player grid

        Debug.Log("Player Ship placement confirmed");
        Debug.Log("Starting Battle...");
        phaseState = PHASE_STATE.PLAYER_FIRING;
        Debug.Log("Phase changed to: PLAYER_FIRING");
        PlayerFires();
    }

    private void PlayerFires()
    {
        // Logic for player firing

        //TODO: deactivate / unhighlight player movement options

        //TODO: activate Fire function on all player ships, resolve hits
        Debug.Log("Player Fired!");
        
        phaseState = PHASE_STATE.ENEMY_FIRING;
        Debug.Log("Phase changed to: ENEMY_FIRING");
        EnemyFires();
    }

    private void EnemyFires()
    {
        // Logic for enemy firing

        //TODO: activate Fire function on all enemy ships, resolve hits

        //TODO: set Wave end conditions if met

        Debug.Log("Enemy Fired!");
        CheckEndWaveConditions();
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

        //TODO: move all enemy ships according to AI rules

        Debug.Log("Enemy is moving...");
        phaseState = PHASE_STATE.PLAYER_MOVING;
        Debug.Log("Phase changed to: PLAYER_MOVING");
        PlayerMoves();
    }

    private void PlayerMoves()
    {
        // Logic for player movement
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
        }
        else
        {
            Debug.Log("Restarting from beginning...");

            waveNumber = 0; // Reset wave number
            loseConditionMet = false; // Reset lose condition
        }

        //TODO: remove all player/enemy ship instances


        phaseState = PHASE_STATE.START_ENCOUNTER;
        Debug.Log("Phase changed to: START_ENCOUNTER");
    }
}