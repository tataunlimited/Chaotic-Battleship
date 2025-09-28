using Core.Board;
using Core.Ship;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class ShipInfoPanel : MonoBehaviour
{
    public Vector2 revealedPosition;
    public Vector2 hiddenPosition;
    public Button toggleButton;
    private bool isVisible = false;
    public bool isPlayerPanel = true; //if false, it's the enemy panel
    public ShipInfoSlotManager infoSlot1;
    public ShipInfoSlotManager infoSlot2;
    public ShipInfoSlotManager infoSlot3;
    public ShipInfoSlotManager infoSlot4;
    public ShipInfoSlotManager infoSlot5;
    public ShipInfoSlotManager infoSlot6;
    public ShipInfoSlotManager infoSlot7;
    public ShipInfoSlotManager infoSlot8;
    public ShipInfoSlotManager infoSlot9;
    public ShipInfoSlotManager infoSlot10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public void ToggleVisibility()
    {
        if (isVisible)
        {
            Hide();
        }
        else
        {
            Reveal();
        }
        isVisible = !isVisible;
    }
    // Update is called once per frame
    void Update()
    {
        // check the gamemanager phase state to populate for the first time
        if (GameManager.instance.phaseState == GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
        {
            Populate();
            Debug.Log("Populated Ship Info Panel in PLAYER_PLACING_SHIPS phase");
        }
    }

    public void Reveal()
    {
        //lerp to revealed position
        StartCoroutine(LerpToPosition(revealedPosition));
        Debug.Log("Revealing Ship Info Panel");
        //reverse toggle button text mesh Z rotation
        toggleButton.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
    }
    public void Hide()
    {
        StartCoroutine(LerpToPosition(hiddenPosition));
        //set toggle button text mesh Z rotation to 180
        toggleButton.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 180);
        Debug.Log("Hiding Ship Info Panel");
    }

    private System.Collections.IEnumerator LerpToPosition(Vector2 targetPosition)
    {
        float elapsedTime = 0f;
        float duration = 0.5f; // Duration of the lerp in seconds
        Vector2 startingPosition = transform.localPosition;

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector2.Lerp(startingPosition, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPosition; // Ensure it ends exactly at the target position
    }

    //hook here to update ship info slots when ships are added/removed
    public void Populate()
    {
        List<ShipView> ships;
        if (!isPlayerPanel)
        {
            ships = new List<ShipView>(BoardController.Instance.enemyView.SpawnedShips);
        }
        else
        {
            ships = new List<ShipView>(BoardController.Instance.playerView.SpawnedShips);
        }
        var infoSlots = new ShipInfoSlotManager[] { infoSlot1, infoSlot2, infoSlot3, infoSlot4, infoSlot5, infoSlot6, infoSlot7, infoSlot8, infoSlot9, infoSlot10 };
        for (int i = 0; i < infoSlots.Length; i++)
        {
            if (i < ships.Count)
            {
                infoSlots[i].SetShip(ships[i]);
                infoSlots[i].gameObject.SetActive(true);
                Debug.Log("Setting ship info slot " + (i + 1) + " to ship of type " + ships[i].shipModel.type + " with HP " + ships[i].shipModel.hp + " and Armor " + ships[i].shipModel.armor);
            }
            else
            {
                infoSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
