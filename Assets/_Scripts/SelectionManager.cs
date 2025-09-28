using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;
    private ShipController currentSelectedShip;

    void Awake()
    {
        Instance = this;
    }

    public void SelectShip(ShipController newShip)
    {
        // Deselect the old ship
        if (currentSelectedShip != null && currentSelectedShip != newShip)
        {
            currentSelectedShip.SetSelected(false);
        }

        // Select the new ship
        currentSelectedShip = newShip;
        currentSelectedShip.SetSelected(true);
    }
}