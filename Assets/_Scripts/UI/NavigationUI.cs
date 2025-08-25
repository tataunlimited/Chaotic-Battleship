using System;
using Core.Board;
using Core.Ship;
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
        }

        public void EnableUI(bool enable)
        {
            gameObject.SetActive(enable);
        }

        public void OnResetButtonClicked()
        {
            // should this be changed to the class having a BoardController attribute that we set in the Unity Inspector?
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