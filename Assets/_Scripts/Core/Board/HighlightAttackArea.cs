using System.Collections.Generic;
using Core.GridSystem;
using Core.Ship;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.Board
{
    public class HighlightAttackArea: MonoBehaviour
    {
        public Image cellImage;
        public Color defaultColor;
        public Color chanceColor;
        public Color selectionColor;
        
        public List<Image> cellImages = new List<Image>();
        
        public void SpawnHighlights(List<GridPos> positions, List<GridPos> selectedCoords, bool chance = false)
        {
            ClearHighlight();

            var cColor = BoardController.Instance.SelectedShip.shipModel.type == ShipType.Destroyer
                ? selectionColor
                : chanceColor;
            
            foreach (var gridPos in positions)
            {
                var img = Instantiate(cellImage, transform);

                img.color = chance ? cColor : defaultColor;
                
                if (selectedCoords.Contains(gridPos))
                {
                    img.color = defaultColor;
                }
                img.GetComponent<RectTransform>().anchoredPosition = new Vector2(gridPos.x, gridPos.y);
                cellImages.Add(img);
            }

            foreach (var pos in selectedCoords)
            {
                
            }
        }

        public void ClearHighlight()
        {
            foreach (var cell in cellImages)
            {
                Destroy(cell.gameObject);
            }
            cellImages.Clear();
        }
        
        
        

    }
}
