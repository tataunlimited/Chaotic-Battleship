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
            // should this be changed to the class having a BoardController attribute that we set in the Unity Inspector?
            BoardController boardController = BoardController.Get();
            boardController.ClearSelectedShip();
            boardController.playerView.ResetMovementPhase();
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
