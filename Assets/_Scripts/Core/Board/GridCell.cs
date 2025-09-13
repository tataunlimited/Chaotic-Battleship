using System;
using Core.GridSystem;
using UnityEngine;

namespace Core.Board
{
    public class GridCell : MonoBehaviour
    {
        private static readonly int Color1 = Shader.PropertyToID("_BaseColor");
        private static readonly int Color2 = Shader.PropertyToID("_Color");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        
        public Renderer frame;
        public Renderer center;

        [Header("Default Color")]
        public Color normalColor;
        [ColorUsage(true, true)] 
        public Color normalColorEmission;
        [Header("Near Miss Color")]
        public Color nearMissColor;
        [ColorUsage(true, true)] 
        public Color nearMissColorEmission;
        [Header("Hit Color")]
        public Color hitColor;
        [ColorUsage(true, true)] 
        public Color hitColorEmission;
        [Header("Miss Color")]
        public Color missColor;
        [ColorUsage(true, true)] 
        public Color missColorEmission;
        



        public void SetColor(CellState state)
        {
            var color = GetColor(state);
            if(frame.material.HasColor(Color1))
                frame.material.SetColor(Color1, color);
            if(frame.material.HasColor(EmissionColor))
                frame.material.SetColor(EmissionColor, GetEmissionColor(state));
            center.gameObject.SetActive(state != CellState.Empty && state != CellState.Ship);
            color = new Color(color.r, color.g, color.b, 0.5f);
            center.material.SetColor(Color2,color); 

        }
        
        private Color GetColor(CellState state)
        {
           switch (state)
            {
                case CellState.Hit:
                    return hitColor;
                case CellState.NearMiss:
                    return nearMissColor;
                case CellState.Miss:
                    return missColor;
            }

            return normalColor;

        }

        private Color GetEmissionColor(CellState state)
        {
            
            switch (state)
            {
                case CellState.Hit:
                    return hitColorEmission;
                case CellState.NearMiss:
                    return nearMissColorEmission;
                case CellState.Miss:
                    return missColorEmission;
            }
            return normalColorEmission;

        }
    }
}
