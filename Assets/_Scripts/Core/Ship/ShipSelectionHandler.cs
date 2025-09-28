using UnityEngine;

namespace Core.Ship
{
	public class ShipSelectionHandler : MonoBehaviour
	{
		public ShipAttackVisualizer visualizer;
		private ShipView _currentlySelectedShip = null;

		// Call this when a ship is clicked (selected)
		public void SelectShip(ShipView ship)
		{
			if (_currentlySelectedShip == ship)
			{
				// Deselect
				_currentlySelectedShip = null;
				visualizer.ClearVisualizations();
			}
			else
			{
				// Select new ship
				_currentlySelectedShip = ship;
				visualizer.VisualizeAttack(ship, true);
			}
		}

		// Unity Event: Called when the mouse pointer enters the ship's collider
		public void OnShipHoverEnter(ShipView ship)
		{
			if (_currentlySelectedShip != ship)
			{
				// Visualize the hovered ship
				visualizer.VisualizeAttack(ship, false);
			}
		}

		// Unity Event: Called when the mouse pointer exits the ship's collider
		public void OnShipHoverExit(ShipView ship)
		{
			if (_currentlySelectedShip != ship)
			{
				// Clear visualization if the ship is not selected
				visualizer.ClearVisualizations();
			}
			else
			{
				// If the ship is selected, re-visualize the selected ship's attack
				visualizer.VisualizeSelectedShip();
			}
		}
	}
}