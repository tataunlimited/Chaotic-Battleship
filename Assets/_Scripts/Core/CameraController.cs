using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera to tween FOV on (optional). If null, will try GetComponent<Camera>() then Camera.main.")]
    public Camera targetCamera;

    [Header("Camera Anchor Positions")]
    public Vector3 defaultPosition;
    public Vector3 defaultEulerAngles;
    public Vector3 attackPosition;
    public Vector3 attackEulerAngles;

    [Header("Transition Settings")]
    [Tooltip("Seconds to move between views when duration not provided.")]
    public float transitionDuration = 0.6f;
    public AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool useUnscaledTime = false;

    [Header("Optional FOV Tween")]
    public bool tweenFieldOfView = false;
    [Range(1f, 179f)] public float defaultFOV = 60f;
    [Range(1f, 179f)] public float attackFOV  = 45f;

    public bool IsTransitioning { get; private set; }

    Coroutine _activeRoutine;

    void Awake()
    {
        if (!targetCamera) {targetCamera = Camera.main;}
        defaultPosition = transform.position;
        defaultEulerAngles = transform.rotation.eulerAngles;
        if (targetCamera) defaultFOV = targetCamera.fieldOfView;
    }


    /// <summary>Lerp to the default view</summary>
    public void GoToDefaultView(bool instant = false)
    {
        Pose pose = GetDefaultPose();
        float dur = instant ? 0f : transitionDuration;
        float? fov = tweenFieldOfView && targetCamera ? defaultFOV : (float?)null;
        LerpToPose(pose, dur, fov);
    }

    /// <summary>Lerp to the attack view (anchor or fallback).</summary>
    public void GoToAttackView(bool instant = false)
    {
        Pose pose = GetAttackPose();
        float dur = instant ? 0f : transitionDuration;
        float? fov = tweenFieldOfView && targetCamera ? attackFOV : (float?)null;
        LerpToPose(pose, dur, fov);
    }

    /// <summary>Immediately snaps to the default view (no tween).</summary>
    public void SnapToDefault() => GoToDefaultView(instant: true);

    /// <summary>Immediately snaps to the attack view (no tween).</summary>
    public void SnapToAttack() => GoToAttackView(instant: true);

    // Convenience to capture current transform as default/attack from the Inspector context menu.
    [ContextMenu("Capture Current As Default")]
    void CaptureCurrentAsDefault()
    {
        defaultPosition = transform.position;
        defaultEulerAngles = transform.rotation.eulerAngles;
        if (targetCamera) defaultFOV = targetCamera.fieldOfView;
    }

    [ContextMenu("Capture Current As Attack")]
    void CaptureCurrentAsAttack()
    {
        attackPosition = transform.position;
        attackEulerAngles = transform.rotation.eulerAngles;
        if (targetCamera) attackFOV = targetCamera.fieldOfView;
    }

    // -------- Internals --------

    private Pose GetDefaultPose()
    {
        return new Pose(defaultPosition, Quaternion.Euler(defaultEulerAngles));
    }

    private Pose GetAttackPose()
    {
        return new Pose(attackPosition, Quaternion.Euler(attackEulerAngles));
    }

    private void LerpToPose(Pose targetPose, float duration, float? fovTarget)
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(Co_LerpToPose(targetPose, duration, fovTarget));
    }

    System.Collections.IEnumerator Co_LerpToPose(Pose targetPose, float duration, float? fovTarget)
    {
        IsTransitioning = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float startFov = 60f;
        if (targetCamera) startFov = targetCamera.fieldOfView;

        if (duration <= 0f)
        {
            transform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
            if (fovTarget.HasValue && targetCamera) targetCamera.fieldOfView = fovTarget.Value;
            IsTransitioning = false;
            _activeRoutine = null;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt / Mathf.Max(duration, 0.0001f);
            float u = easing != null ? easing.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);

            transform.position = Vector3.LerpUnclamped(startPos, targetPose.position, u);
            transform.rotation = Quaternion.SlerpUnclamped(startRot, targetPose.rotation, u);

            if (fovTarget.HasValue && targetCamera)
                targetCamera.fieldOfView = Mathf.LerpUnclamped(startFov, fovTarget.Value, u);

            yield return null;
        }

        transform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
        if (fovTarget.HasValue && targetCamera) targetCamera.fieldOfView = fovTarget.Value;
        IsTransitioning = false;
        _activeRoutine = null;
    }
}