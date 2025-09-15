using UnityEngine;

public class UpgradeShopPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private bool pauseOnOpen = true;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Toggle()
    {
        if (panel == null) return;
        bool show = !panel.activeSelf;
        panel.SetActive(show);
        if (pauseOnOpen) Time.timeScale = show ? 0f : 1f;
    }

    public void Open()
    {
        if (panel == null) return;
        panel.SetActive(true);
        if (pauseOnOpen) Time.timeScale = 0f;
    }

    public void Close()
    {
        if (panel == null) return;
        panel.SetActive(false);
        if (pauseOnOpen) Time.timeScale = 1f;
    }
}
