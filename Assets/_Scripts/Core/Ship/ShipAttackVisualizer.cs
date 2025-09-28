using System.Collections.Generic;
using UnityEngine;
using Core.Board;
using Core.GridSystem;

namespace Core.Ship
{
	public class ShipAttackVisualizer : MonoBehaviour
	{
		// --- Public Customizable Fields ---
		[Header("General Settings")]
		[Tooltip("The Enemy BoardView to draw visualizations on.")]
		public BoardView enemyBoard;

		[Header("Visual Prefabs")]
		[Tooltip("Sleek visual for guaranteed attack cells (e.g., a pulsing glow decal).")]
		public GameObject guaranteedHighlightPrefab;
		[Tooltip("Sleek visual for chance attack cells (e.g., a subtle fading decal).")]
		public GameObject chanceHighlightPrefab;
		[Tooltip("Visual for the Destroyer's target point (e.g., a crosshair).")]
		public GameObject destroyerTargetPrefab;
		[Tooltip("Line Renderer for the Destroyer's targeting line.")]
		public LineRenderer destroyerTargetLinePrefab;

		// --- Private State ---
		private List<GameObject> _activeHighlights = new List<GameObject>();
		private GameObject _activeTargetIndicator;
		private LineRenderer _activeTargetLine;

		// UI for bonus criteria
		public GameObject attackInfoUIPrefab;
		private GameObject _activeInfoUI;

		private ShipView _selectedShip = null;

		// --- Public Methods ---

		// 1. Core method to draw/update the visualization
		public void VisualizeAttack(ShipView ship, bool isSelected)
		{
			ClearVisualizations();

			// Only visualize if the ship is the player's and not sunk
			if (!ship.IsPlayer || ship.shipModel.IsSunk) return;

			// Get attack data from the ShipModel
			List<GridPos> allPossibleCoords = ship.shipModel.GetPossibleAreaOfAttack(enemyBoard, out List<GridPos> selectedCoords, out bool isChance);

			// --- BONUS UI POPUP ---
			ShowShipInfoUI(ship);
			// ------------------------

			if (ship.shipModel.type == ShipType.Destroyer)
			{
				// Special handling for Destroyer's target point and line
				DrawDestroyerTarget(ship, selectedCoords[0]); // Destroyer targets only one cell
			}
			else
			{
				// Draw all general attack highlights (Battleship, Submarine, Cruiser)
				DrawGeneralHighlights(allPossibleCoords, isChance);
			}

			if (isSelected)
			{
				_selectedShip = ship;
			}
		}

		// 2. Clear all active visuals
		public void ClearVisualizations()
		{
			foreach (var highlight in _activeHighlights)
			{
				Destroy(highlight);
			}
			_activeHighlights.Clear();

			if (_activeTargetIndicator != null)
			{
				Destroy(_activeTargetIndicator);
				_activeTargetIndicator = null;
			}

			if (_activeTargetLine != null)
			{
				Destroy(_activeTargetLine.gameObject);
				_activeTargetLine = null;
			}

			HideShipInfoUI();
		}

		// 3. Re-visualize the selected ship (used when mouse leaves a *different* ship)
		public void VisualizeSelectedShip()
		{
			if (_selectedShip != null)
			{
				VisualizeAttack(_selectedShip, true);
			}
		}

		// --- Private Drawing Methods ---

		private void DrawGeneralHighlights(List<GridPos> coords, bool isChance)
		{
			// Choose the prefab based on whether the attack is "guaranteed" or "chance"
			GameObject highlightPrefab = isChance ? chanceHighlightPrefab : guaranteedHighlightPrefab;

			foreach (var pos in coords)
			{
				Vector3 worldPos = enemyBoard.GridToWorld(pos, 0.05f); // 0.05f is a small offset to float above the grid
				GameObject highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity, transform);
				_activeHighlights.Add(highlight);
			}
		}
		private void DrawDestroyerTarget(ShipView destroyer, GridPos targetPos)
		{
			// a) Draw the Target Indicator (Crosshair/Circle)
			Vector3 targetWorldPos = enemyBoard.GridToWorld(targetPos, 0.05f);
			_activeTargetIndicator = Instantiate(destroyerTargetPrefab, targetWorldPos, Quaternion.identity, transform);

			// b) Draw the Visual Line extending from the Destroyer
			Vector3 destroyerBowPos = destroyer.torpedoSpawnPoint.position; // Use the bow/spawn point

			// Instantiate the Line Renderer
			_activeTargetLine = Instantiate(destroyerTargetLinePrefab, transform);

			// Set the start and end points of the line
			_activeTargetLine.positionCount = 2;
			_activeTargetLine.SetPosition(0, destroyerBowPos);
			_activeTargetLine.SetPosition(1, targetWorldPos);
		}

		// --- Bonus UI Methods ---

		private void ShowShipInfoUI(ShipView ship)
		{
			string info = $"**{ship.shipModel.type}** Attack Pattern: ";
			if (ship.shipModel.type == ShipType.Submarine)
			{
				info += "Firing along the current line (or reloading on next round).";
			}
			else if (ship.shipModel.type == ShipType.Destroyer)
			{
				info += "Targeting a specific cell with high damage.";
			}
			// ... add logic for other ships

			if (_activeInfoUI == null && attackInfoUIPrefab != null)
			{
				_activeInfoUI = Instantiate(attackInfoUIPrefab, transform.parent);
			}
		}

		private void HideShipInfoUI()
		{
			if (_activeInfoUI != null)
			{
				_activeInfoUI.SetActive(false);
			}
		}
	}
}