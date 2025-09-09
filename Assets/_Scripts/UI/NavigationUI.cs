using Core.Board;
using TMPro;
using UnityEngine;

namespace UI
{
    public class NavigationUI : MonoBehaviour
    {
        public TMP_Text shipMovementRemaining;
        private BoardController _boardController;

        private void Start()
        {
            _boardController = BoardController.Instance;
            _boardController.OnShipSelected += EnableUI;
            EnableUI(false);
        }

        public void EnableUI(bool enable)
        {
            gameObject.SetActive(enable);
        }

        public void OnResetButtonClicked()
        {
            if (GameManager.instance.phaseState == GameManager.PHASE_STATE.PLAYER_PLACING_SHIPS)
                return;     // was causing bug during PLAYER_PLACING_SHIPS, so just doing nothing then

            _boardController.ClearSelectedShip();
            _boardController.playerView.ResetMovementPhase();
        }

        public void UpdateShipMovementRemaining(int value)
        {
            shipMovementRemaining.text = value.ToString();
        }

        public void RotateLeft()
        {
            if (_boardController.SelectedShip != null)
            {
                if (_boardController.SelectedShip.RotateLeft())
                {
                    _boardController.ClearSelectedShip();
                }
            }
        }

        public void RotateRight()
        {
            if (_boardController.SelectedShip != null)
            {
                if (_boardController.SelectedShip.RotateRight())
                {
                    _boardController.ClearSelectedShip();
                }
            }
        }
    }
}