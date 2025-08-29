using System;
using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Board
{
    public class MovementCellManager : MonoBehaviour
    {
        public Button cellButtonPrefab;
        
        public List<Button> cellButtons = new List<Button>();



        public void SpawnCell(GridPos gridPos, Action onCellClicked)
        {
            var button = Instantiate(cellButtonPrefab, transform);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(gridPos.x, gridPos.y);
            button.onClick.AddListener(() => onCellClicked?.Invoke());
            cellButtons.Add(button);
        }

        public void ClearCells()
        {
            foreach (var cell in cellButtons)
            {
                Destroy(cell.gameObject);
            }
            cellButtons.Clear();
        }
    }
}
