using Core.Board;
using Core.Ship;
using TMPro;
using UnityEngine;

namespace UI
{
    public class NavigationUI : MonoBehaviour
    {
        public TMP_Text shipMovementRemaining;


        public void EnableUI(bool enable)
        {
            gameObject.SetActive(enable);
        }

        public void OnResetButtonClicked()
        {
            
        }

        public void UpdateShipMovementRemaining(int value)
        {
            shipMovementRemaining.text = value.ToString();
        }

        public void RotateLeft()
        {
            if (BoardController.SelectedShip != null)
            {
                BoardController.SelectedShip.RotateLeft();
            }
        }

        public void RotateRight()
        {
            if (BoardController.SelectedShip != null)
            {
                BoardController.SelectedShip.RotateRight();
            }
        }

    }
}
