using System.Collections.Generic;
using Core.GridSystem;
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
        
        public List<Image> cellImages = new List<Image>();
        
        public void SpawnHighlights(List<GridPos> positions, bool chance = false)
        {
            ClearHighlight();
            foreach (var gridPos in positions)
            {
                var img = Instantiate(cellImage, transform);
                img.color = chance ? chanceColor : defaultColor;
                img.GetComponent<RectTransform>().anchoredPosition = new Vector2(gridPos.x, gridPos.y);
                cellImages.Add(img);
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
