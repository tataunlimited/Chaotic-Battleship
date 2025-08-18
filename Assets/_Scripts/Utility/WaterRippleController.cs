using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.LowLevel; // PointerId
#endif


[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class WaterRippleController : MonoBehaviour
{
    [Header("Shader & Rendering")]
    [Tooltip("Leave empty to auto-use this object's renderer material")]
    public Material waterMat;                          // optional (we'll still use MPB on the Renderer)
    private Renderer rend;
    private MaterialPropertyBlock mpb;

    [Header("Camera / Input")]
    public Camera mainCamera;                          // assign or auto-find Camera.main
    public bool ignoreWhenOverUI = true;

    [Header("Ripple Settings (defaults when clicking)")]
    [Range(0, 8)] public int maxRipples = 8;
    public float defaultMaxRadius = 8f;
    public float defaultAmplitude = 1.2f;
    public float defaultFrequency = 1.8f;
    public float defaultDamping = 0.7f;

    [Header("Debug")]
    public bool debugLogs = false;
    public bool debugGizmos = true;

    struct Ripple
    {
        public Vector2 xz;
        public float startTime;
        public Vector4 data;   // x=maxRadius, y=amp, z=freq, w=damp
        public bool alive;
    }
    private Ripple[] slots = new Ripple[8];

    // Shader property IDs
    static readonly int _RippleCount = Shader.PropertyToID("_RippleCount");
    static readonly int[] _posIDs = {
        Shader.PropertyToID("_RipplePos0"), Shader.PropertyToID("_RipplePos1"),
        Shader.PropertyToID("_RipplePos2"), Shader.PropertyToID("_RipplePos3"),
        Shader.PropertyToID("_RipplePos4"), Shader.PropertyToID("_RipplePos5"),
        Shader.PropertyToID("_RipplePos6"), Shader.PropertyToID("_RipplePos7")
    };
    static readonly int[] _dataIDs = {
        Shader.PropertyToID("_RippleData0"), Shader.PropertyToID("_RippleData1"),
        Shader.PropertyToID("_RippleData2"), Shader.PropertyToID("_RippleData3"),
        Shader.PropertyToID("_RippleData4"), Shader.PropertyToID("_RippleData5"),
        Shader.PropertyToID("_RippleData6"), Shader.PropertyToID("_RippleData7")
    };

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (mainCamera == null) mainCamera = Camera.main;

        mpb ??= new MaterialPropertyBlock();

        // OPTIONAL sanity test:
        AddRipple(transform.position + Vector3.right * 0.01f);
        ignoreWhenOverUI = false;
    }

    bool IsPointerOverUI()
    {
        if (!ignoreWhenOverUI || EventSystem.current == null) return false;

        // New Input System path
    #if ENABLE_INPUT_SYSTEM
        var uiModule = EventSystem.current.currentInputModule as InputSystemUIInputModule;
        if (uiModule != null)
        {
            // Mouse pointer id
            return uiModule.IsPointerOverGameObject(PointerId.mousePointerId);
        }
    #endif
        // Old Standalone Input Module path
        return EventSystem.current.IsPointerOverGameObject();
    }
    void Update()
    {
        // Mouse click?
        if (Application.isPlaying && Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
            {
                if (debugLogs) Debug.Log("Click over UI ignored.");
            }
            else
            {
                TrySpawnRippleFromMouse();
            }
        }

        // Push all ripple data to material each frame (also expires old ones)
        PushToRenderer();
        ExpireOldRipples();
    }

    void TrySpawnRippleFromMouse()
    {
        if (!mainCamera)
        {
            if (debugLogs) Debug.LogWarning("No mainCamera set/found.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Prefer physics raycast against this water's collider (works for rotated/tilted surfaces)
        Collider col = GetComponent<Collider>();
        if (col && col.Raycast(ray, out RaycastHit hit, 5000f))
        {
            AddRipple(hit.point);
            Debug.Log($"[Ripple] Click @ {hit.point} (XZ = {hit.point.x:F2}, {hit.point.z:F2})");
            if (debugLogs) Debug.Log($"Ripple @ {hit.point}");
            return;
        }

        // Fallback: infinite plane at this transform (uses local up & position)
        Plane plane = new Plane(transform.up, transform.position);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 p = ray.GetPoint(enter);
            AddRipple(p);
            if (debugLogs) Debug.Log($"Ripple (plane) @ {p}");
        }
        else if (debugLogs)
        {
            Debug.Log("Ray missed both collider and plane.");
        }
    }

    public void AddRipple(Vector3 worldPos)
    {
        AddRipple(worldPos, defaultMaxRadius, defaultAmplitude, defaultFrequency, defaultDamping);
    }

    public void AddRipple(Vector3 worldPos, float maxRadius, float amplitude, float frequency, float damping)
    {
        // Choose a free slot or overwrite oldest
        int slot = -1;
        float oldestStart = float.MaxValue;
        float now = Time.time;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].alive) { slot = i; break; }
            if (slots[i].startTime < oldestStart) { oldestStart = slots[i].startTime; slot = i; }
        }

        slots[slot].xz = new Vector2(worldPos.x, worldPos.z);
        slots[slot].startTime = now;
        slots[slot].data = new Vector4(maxRadius, amplitude, frequency, damping);
        slots[slot].alive = true;
    }

    void PushToRenderer()
    {
        // Use MPB so we update the actual instance used by this Renderer (SRP-batcher-friendly)
        rend.GetPropertyBlock(mpb);

        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].alive)
            {
                mpb.SetVector(_posIDs[i], new Vector4(slots[i].xz.x, slots[i].xz.y, slots[i].startTime, 0));
                mpb.SetVector(_dataIDs[i], slots[i].data);
                count++;
            }
            else
            {
                mpb.SetVector(_posIDs[i], Vector4.zero);
                mpb.SetVector(_dataIDs[i], new Vector4(0, 0, 0, 1));
            }
        }
        mpb.SetInt(_RippleCount, Mathf.Min(count, maxRipples));
        rend.SetPropertyBlock(mpb);

        // Also set on a material reference if you’re driving a shared material elsewhere (optional)
        if (waterMat)
        {
            waterMat.SetInt(_RippleCount, Mathf.Min(count, maxRipples));
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].alive)
                {
                    waterMat.SetVector(_posIDs[i], new Vector4(slots[i].xz.x, slots[i].xz.y, slots[i].startTime, 0));
                    waterMat.SetVector(_dataIDs[i], slots[i].data);
                }
                else
                {
                    waterMat.SetVector(_posIDs[i], Vector4.zero);
                    waterMat.SetVector(_dataIDs[i], new Vector4(0,0,0,1));
                }
            }
        }
    }

    void ExpireOldRipples()
    {
        float now = Time.time;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].alive) continue;
            float age = now - slots[i].startTime;
            float maxR = slots[i].data.x;
            float damp = slots[i].data.w;

            // simple expiry heuristic
            if (age > (maxR * 2f) || age * damp > 8f)
                slots[i].alive = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!debugGizmos) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].alive) continue;
            var p = new Vector3(slots[i].xz.x, transform.position.y + 0.05f, slots[i].xz.y);
            Gizmos.DrawWireSphere(p, 0.2f);
        }
    }
}