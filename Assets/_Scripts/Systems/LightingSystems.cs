using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightingSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Light sunLight;          // Existing Directional Light in Game scene
    [SerializeField] private Volume globalVolume;     // Existing Global Volume in Game scene

    [Header("Day Preset")]
    [SerializeField] private VolumeProfile dayProfile;
    [SerializeField] private Color  daySunColor     = new Color(1f, 0.956f, 0.84f);
    [SerializeField] private float  daySunIntensity = 1.2f;
    [SerializeField] private Material daySkybox;
    [SerializeField] private Color  dayAmbientColor = new Color(0.60f, 0.60f, 0.70f);

    [Header("Night Preset")]
    [SerializeField] private VolumeProfile nightProfile;
    [SerializeField] private Color  nightSunColor     = new Color(0.50f, 0.60f, 1.00f);
    [SerializeField] private float  nightSunIntensity = 0.05f;
    [SerializeField] private Material nightSkybox;
    [SerializeField] private Color  nightAmbientColor = new Color(0.02f, 0.02f, 0.05f);

    public enum Mode { Day, Night }
    [SerializeField] private Mode startMode = Mode.Day;
    public Mode CurrentMode { get; private set; }

    void Awake()
    {
        if (!sunLight)     sunLight     = RenderSettings.sun;
        if (!globalVolume) globalVolume = FindObjectOfType<Volume>();
    }

    void Start() => Apply(startMode);

    public void SetDayMode()  => Apply(Mode.Day);
    public void SetNightMode() => Apply(Mode.Night);
    public void Toggle() => Apply(CurrentMode == Mode.Day ? Mode.Night : Mode.Day);

    private void Apply(Mode mode)
    {
        CurrentMode = mode;

        if (globalVolume)
            globalVolume.profile = (mode == Mode.Day) ? dayProfile : nightProfile;

        if (sunLight)
        {
            if (mode == Mode.Day)
            {
                sunLight.color     = daySunColor;
                sunLight.intensity = daySunIntensity;
                sunLight.shadows   = LightShadows.Soft;
            }
            else
            {
                sunLight.color     = nightSunColor;
                sunLight.intensity = nightSunIntensity;
                sunLight.shadows   = LightShadows.Soft;
            }
        }

        if (mode == Mode.Day)
        {
            if (daySkybox) RenderSettings.skybox = daySkybox;
            RenderSettings.ambientMode  = AmbientMode.Flat;
            RenderSettings.ambientLight = dayAmbientColor;
        }
        else
        {
            if (nightSkybox) RenderSettings.skybox = nightSkybox;
            RenderSettings.ambientMode  = AmbientMode.Flat;
            RenderSettings.ambientLight = nightAmbientColor;
        }

        DynamicGI.UpdateEnvironment();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
            Apply(CurrentMode);
    }
#endif
}
