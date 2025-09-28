using UnityEngine;

public class ShipController : MonoBehaviour
{
    public string shipID; // e.g., "Destroyer_1", used for grouping all segments
    public Material defaultMaterial;
    public Material hoverMaterial;
    public Material selectedMaterial;

    private MeshRenderer[] segmentRenderers;
    private bool isSelected = false;

    void Start()
    {
        // Get the renderers for all segments of the ship
        // Assuming the ship is made of multiple child GameObjects (segments)
        segmentRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }

    public void SetHovered(bool hovered)
    {
        // Only update hover if the ship isn't already selected
        if (!isSelected)
        {
            UpdateVisuals(hovered);
        }
    }

    private void UpdateVisuals(bool isHovered = false)
    {
        Material targetMat;

        if (isSelected)
        {
            // Bonus: Extra highlighted when selected
            targetMat = selectedMaterial;
        }
        else if (isHovered)
        {
            // Highlighted when hovered
            targetMat = hoverMaterial;
        }
        else
        {
            // Default appearance
            targetMat = defaultMaterial;
        }

        // Apply the material to all segments
        foreach (var renderer in segmentRenderers)
        {
            renderer.material = targetMat;
        }
    }
}