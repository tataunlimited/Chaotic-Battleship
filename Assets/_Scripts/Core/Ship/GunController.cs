using System.Collections;
using System.Collections.Generic;
using Core.GridSystem;
using UnityEngine;

namespace Core.Ship
{
    public class GunController : MonoBehaviour
    {
        [Header("Rig")]
        public Transform pivotYaw;           // rotates around local Y
        public Transform pivotPitch;         // rotates around local X (child of pivotYaw)
        [Tooltip("All muzzle tips for this turret (one per barrel).")]
        public List<Transform> muzzles = new();

        [Header("Aiming")]
        public float yawSpeedDeg = 120f;
        public float pitchSpeedDeg = 90f;

        [Tooltip("Clamp local Y (deg) around the turret's base. Untick to allow 360°.")]
        public bool useYawLimits = false;
        public float minYawLocal = -150f;
        public float maxYawLocal = 150f;

        [Tooltip("Clamp local X (deg) for pitch. Up is positive.")]
        public bool usePitchLimits = true;
        public float minPitchLocal = -5f;    // down
        public float maxPitchLocal = 35f;    // up

        [Tooltip("Tolerance (deg) applied to BOTH yaw & pitch errors for 'aimed' condition.")]
        public float aimToleranceDeg = 2.0f;

        [Tooltip("When remaining delta is within this many degrees, snap exactly to goal so we 'lock'.")]
        public float settleSnapDeg = 0.3f;

        [Header("Aiming Variation")]
        [Tooltip("±% random speed variation at shot start, easing back to 1.0 over JitterDuration.")]
        [Range(0f, 0.5f)] public float speedJitterPercent = 0.15f;
        [Tooltip("Seconds for jittered speed to blend back to normal.")]
        [Range(0f, 1.5f)] public float speedJitterDuration = 0.4f;

        [Header("Forward Reference")]
        [Tooltip("What transform defines the barrel's forward? If null, uses first muzzle; else pivotPitch.")]
        public Transform barrelForwardRef;
        [Tooltip("Auto-detect if the barrel uses -Z instead of +Z and flip once at runtime.")]
        public bool autoDetectForwardFlip = true;
        [Tooltip("Manually flip the forward direction (+Z vs -Z).")]
        public bool invertForward = false;

        [Header("Muzzle FX")]
        public ParticleSystem muzzleFlashPrefab;
        public bool matchMuzzleRotation = true;

        [Header("Audio")]
        [Tooltip("If assigned, used for PlayOneShot. If null, an AudioSource is auto-created.")]
        public AudioSource audioSource;
        public AudioClip fireSfx;
        public bool autoWireAudioSource = true;
        public bool playAtEachMuzzle = true;
        [Range(0f,1f)] public float sfxVolume = 1f;
        public float audioMinDistance = 4f;
        public float audioMaxDistance = 60f;

        // runtime
        Transform _root;
        bool _forwardCalibrated;
        float _currentYawSpeed   = 120f;
        float _currentPitchSpeed = 90f;

        void Awake()
        {
            _root = (pivotYaw != null && pivotYaw.parent != null) ? pivotYaw.parent : transform;

            if (barrelForwardRef == null)
                barrelForwardRef = (muzzles != null && muzzles.Count > 0) ? muzzles[0] : pivotPitch;

            EnsureAudioSource();

            // init runtime speeds to inspector values
            _currentYawSpeed   = yawSpeedDeg;
            _currentPitchSpeed = pitchSpeedDeg;
        }

        void EnsureAudioSource()
        {
            if (!autoWireAudioSource) return;
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>()
                               ?? (pivotPitch ? pivotPitch.GetComponent<AudioSource>() : null)
                               ?? (pivotYaw ? pivotYaw.GetComponent<AudioSource>() : null)
                               ?? GetComponentInChildren<AudioSource>();

                if (audioSource == null)
                {
                    var host = pivotPitch != null ? pivotPitch.gameObject : gameObject;
                    audioSource = host.AddComponent<AudioSource>();
                }
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = audioMinDistance;
            audioSource.maxDistance = audioMaxDistance;
            audioSource.dopplerLevel = 0f;
        }

        // ---------- Aiming core ----------
        public void AimAt(Vector3 targetWorld, float deltaTime)
        {
            if (pivotYaw == null || pivotPitch == null || barrelForwardRef == null) return;

            Vector3 toTargetW = (targetWorld - barrelForwardRef.position);
            if (toTargetW.sqrMagnitude < 1e-8f) return;

            // One-time auto-calibration (+Z vs -Z authored forward)
            if (autoDetectForwardFlip && !_forwardCalibrated)
            {
                float aPos = Vector3.Angle(barrelForwardRef.forward, toTargetW);
                float aNeg = Vector3.Angle(-barrelForwardRef.forward, toTargetW);
                if (aNeg + 0.5f < aPos) invertForward = true;
                _forwardCalibrated = true;
            }

            // Compute deltas using actual barrel forward
            float yawDelta, pitchDelta;
            ComputeYawPitchDelta(targetWorld, out yawDelta, out pitchDelta);

            // ----- YAW STEP -----
            var yEuler = pivotYaw.localEulerAngles; // preserve X/Z
            float yawStep = Mathf.Clamp(yawDelta, -_currentYawSpeed * deltaTime, _currentYawSpeed * deltaTime);

            if (Mathf.Abs(yawDelta) <= settleSnapDeg) // tiny residual -> snap
                yEuler.y = ClampIf(useYawLimits, yEuler.y + yawDelta, minYawLocal, maxYawLocal);
            else
                yEuler.y = ClampIf(useYawLimits, yEuler.y + yawStep,  minYawLocal, maxYawLocal);

            pivotYaw.localEulerAngles = yEuler;

            // ----- PITCH STEP -----
            var pEuler = pivotPitch.localEulerAngles; // preserve Y/Z
            float pitchStep = Mathf.Clamp(pitchDelta, -_currentPitchSpeed * deltaTime, _currentPitchSpeed * deltaTime);

            if (Mathf.Abs(pitchDelta) <= settleSnapDeg)
                pEuler.x = ClampIf(usePitchLimits, pEuler.x + pitchDelta, minPitchLocal, maxPitchLocal);
            else
                pEuler.x = ClampIf(usePitchLimits, pEuler.x + pitchStep,  minPitchLocal, maxPitchLocal);

            pivotPitch.localEulerAngles = pEuler;
        }

        /// <summary>Return true if BOTH yaw & pitch errors are within tolerance.</summary>
        public bool IsAimedAt(Vector3 targetWorld)
        {
            float yawErrAbs, pitchErrAbs;
            ComputeYawPitchError(targetWorld, out yawErrAbs, out pitchErrAbs);
            return yawErrAbs <= aimToleranceDeg && pitchErrAbs <= aimToleranceDeg;
        }

        // ---------- Fire ----------
        public void FireOnce()
        {
            // VFX
            if (muzzles != null && muzzles.Count > 0 && muzzleFlashPrefab != null)
            {
                foreach (var m in muzzles)
                {
                    if (!m) continue;
                    var ps = Instantiate(muzzleFlashPrefab,
                                         m.position,
                                         matchMuzzleRotation ? m.rotation : Quaternion.identity);
                    var main = ps.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    ps.Play();
                    Destroy(ps.gameObject, main.duration + main.startLifetime.constantMax + 0.25f);
                }
            }

            // SFX
            if (fireSfx != null)
            {
                if (playAtEachMuzzle && muzzles != null && muzzles.Count > 0)
                {
                    foreach (var m in muzzles)
                    {
                        if (!m) continue;
                        AudioSource.PlayClipAtPoint(fireSfx, m.position, sfxVolume);
                    }
                }
                else
                {
                    EnsureAudioSource();
                    if (audioSource != null)
                    {
                        if (pivotPitch) audioSource.transform.position = pivotPitch.position;
                        audioSource.PlayOneShot(fireSfx, sfxVolume);
                    }
                }
            }
        }

        /// <summary>
        /// Aim at a BoardView cell until yaw & pitch within tolerance (or timeout), then fire.
        /// Includes per-shot speed jitter that blends back to normal.
        /// </summary>
        public IEnumerator AimAndFireRoutine(Board.BoardView board,
                                             GridPos cell,
                                             float yOffset,
                                             float preFireDelay = 0.03f,
                                             float maxAimTime   = 2.0f,
                                             float? jitterPct   = null,
                                             float? jitterDur   = null)
        {
            float t = 0f;

            // --- per-shot jitter setup ---
            float jpct = Mathf.Max(0f, jitterPct  ?? speedJitterPercent);
            float jdur = Mathf.Max(0f, jitterDur  ?? speedJitterDuration);

            float baseYaw   = yawSpeedDeg;
            float basePitch = pitchSpeedDeg;
            float yawJitterScale   = 1f + Random.Range(-jpct, jpct);
            float pitchJitterScale = 1f + Random.Range(-jpct, jpct);

            _currentYawSpeed   = baseYaw   * yawJitterScale;
            _currentPitchSpeed = basePitch * pitchJitterScale;

            try
            {
                while (true)
                {
                    Vector3 targetWorld = board.GridToWorld(cell, yOffset);

                    // ease speeds back to base over jdur
                    if (jdur > 0f && t < jdur)
                    {
                        float k = Mathf.Clamp01(t / jdur);
                        k = k * k * (3f - 2f * k); // smoothstep
                        _currentYawSpeed   = Mathf.Lerp(baseYaw   * yawJitterScale, baseYaw,   k);
                        _currentPitchSpeed = Mathf.Lerp(basePitch * pitchJitterScale, basePitch, k);
                    }
                    else
                    {
                        _currentYawSpeed   = baseYaw;
                        _currentPitchSpeed = basePitch;
                    }

                    AimAt(targetWorld, Time.deltaTime);

                    if (IsAimedAt(targetWorld) || t >= maxAimTime)
                        break;

                    t += Time.deltaTime;
                    yield return null;
                }

                if (preFireDelay > 0f)
                    yield return new WaitForSeconds(preFireDelay);

                FireOnce();
            }
            finally
            {
                _currentYawSpeed   = baseYaw;
                _currentPitchSpeed = basePitch;
            }
        }

        // ---- math helpers ----

        // Component-wise errors (absolute): how much yaw/pitch needed from current pose
        void ComputeYawPitchError(Vector3 targetWorld, out float yawAbs, out float pitchAbs)
        {
            float yawDelta, pitchDelta;
            ComputeYawPitchDelta(targetWorld, out yawDelta, out pitchDelta);
            yawAbs = Mathf.Abs(yawDelta);
            pitchAbs = Mathf.Abs(pitchDelta);
        }

        // Signed deltas to reach target this frame (positive rotates in axis' positive direction)
        void ComputeYawPitchDelta(Vector3 targetWorld, out float yawDelta, out float pitchDelta)
        {
            yawDelta = 0f; pitchDelta = 0f;

            Vector3 toTargetW = targetWorld - barrelForwardRef.position;
            if (toTargetW.sqrMagnitude < 1e-8f) return;

            Vector3 barrelFwdW = (invertForward ? -barrelForwardRef.forward : barrelForwardRef.forward);

            // Yaw: project onto plane orthogonal to up
            Vector3 yawUpW = pivotYaw.up;
            Vector3 curYawDirW = Vector3.ProjectOnPlane(barrelFwdW, yawUpW).normalized;
            Vector3 tgtYawDirW = Vector3.ProjectOnPlane(toTargetW, yawUpW).normalized;

            if (curYawDirW.sqrMagnitude > 1e-8f && tgtYawDirW.sqrMagnitude > 1e-8f)
                yawDelta = Vector3.SignedAngle(curYawDirW, tgtYawDirW, yawUpW);

            // Pitch: use current barrel right-ish axis (perpendicular to up & fwd)
            Vector3 pitchAxisW = Vector3.Cross(yawUpW, barrelFwdW).normalized;
            if (pitchAxisW.sqrMagnitude < 1e-8f) pitchAxisW = pivotPitch.right;

            Vector3 curPitchDirW = Vector3.ProjectOnPlane(barrelFwdW, pitchAxisW).normalized;
            Vector3 tgtPitchDirW = Vector3.ProjectOnPlane(toTargetW,   pitchAxisW).normalized;

            if (curPitchDirW.sqrMagnitude > 1e-8f && tgtPitchDirW.sqrMagnitude > 1e-8f)
                pitchDelta = Vector3.SignedAngle(curPitchDirW, tgtPitchDirW, pitchAxisW);
        }

        // Clamp optionally in degrees, normalizing to [-180,180]
        static float ClampIf(bool doClamp, float angle, float min, float max)
        {
            if (!doClamp) return angle;
            angle = Mathf.DeltaAngle(0f, angle);
            return Mathf.Clamp(angle, min, max);
        }
    }
}
