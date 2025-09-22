using UnityEngine;

public class LightingHotkey : MonoBehaviour
{
    [SerializeField] private LightingSystem lighting;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) lighting.SetDayMode();
        if (Input.GetKeyDown(KeyCode.F6)) lighting.SetNightMode();
        if (Input.GetKeyDown(KeyCode.F7)) lighting.Toggle();
    }
}
