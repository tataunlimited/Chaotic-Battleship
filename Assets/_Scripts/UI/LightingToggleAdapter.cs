using UnityEngine;
using UnityEngine.UI;

public class LightingToggleAdapter : MonoBehaviour
{
    [SerializeField] private LightingSystem lighting;
    [SerializeField] private Toggle toggle;

    private void Reset()
    {
        if (!toggle) toggle = GetComponent<Toggle>();
        if (!lighting) lighting = FindObjectOfType<LightingSystem>();
    }

    private void OnEnable()
    {
        if (toggle == null) return;
        if (lighting != null)
            toggle.isOn = lighting.CurrentMode == LightingSystem.Mode.Night;

        toggle.onValueChanged.AddListener(OnChanged);
    }

    private void OnDisable()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnChanged);
    }

    private void OnChanged(bool isOn)
    {
        if (lighting == null) return;
        if (isOn) lighting.SetNightMode();
        else      lighting.SetDayMode();
    }
}
