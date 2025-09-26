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
        
        private Dictionary<GridPos, Image> _cellHighlights = new();
        


        public void SpawnHighlights(ShipModel shipModel)
        {
            var aot = shipModel.GetPossibleAreaOfAttack(BoardController.Instance.enemyView);
            ClearHighlight();

            foreach (var targetableCell in aot.TargetableCells)
            {
                var img = Instantiate(cellImage, transform);
                img.color = selectionColor;
                SetCellPosition(targetableCell, img);
                _cellHighlights.Add(targetableCell, img);
            }

            foreach (var possibleCell in aot.PossibleCells)
            {
                if (_cellHighlights.TryGetValue(possibleCell, out var highlight))
                {
                    highlight.color = chanceColor;
                }
                else
                {
                    var img = Instantiate(cellImage, transform);
                    img.color = chanceColor;
                    SetCellPosition(possibleCell, img);
                    _cellHighlights.Add(possibleCell, img);
                }
            }

            foreach (var defCell in aot.LineOfFireCells)
            {
                if (_cellHighlights.TryGetValue(defCell, out var highlight))
                {
                    highlight.color = defaultColor;
                }
                else
                {
                    var img = Instantiate(cellImage, transform);
                    img.color = defaultColor;
                    SetCellPosition(defCell, img);
                    _cellHighlights.Add(defCell, img);
                }
            }
        }

        private void SetCellPosition(GridPos pos, Image img)
        {
            img.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, pos.y);
        }
        public void ClearHighlight()
        {

            foreach (var cell in _cellHighlights)
            {
                Destroy(cell.Value.gameObject);
            }
            _cellHighlights.Clear();
        }
        
        
        

    }
}
