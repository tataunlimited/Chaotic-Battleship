using System.Collections;
using System.Collections.Generic;
using Core.Board;
using Core.GridSystem;
using UnityEngine;

namespace Core.Ship
{
    [AddComponentMenu("Game/Ship Gunnery Controller")]
    public class ShipGunneryController : MonoBehaviour
    {
        [Header("Turrets on this ship")]
        public List<GunController> turrets = new();

        [Header("Target Board (default: BoardController.Instance.enemyView)")]
        public BoardView enemyBoard;

        [Header("Volley Timing")]
        [Tooltip("Base delay added before each turret fires (after aim).")]
        public float basePreFireDelay = 0.03f;

        [Tooltip("Extra delay per turret index to create a stagger (0 = simultaneous).")]
        public float perTurretStagger = 0.05f;

        [Tooltip("Each turret will fire after this much time even if not perfectly aimed.")]
        public float maxAimTime = 2.0f;

        [Tooltip("Vertical offset for the grid cell world point (e.g., turret height).")]
        public float targetYOffset = 0f;

        [Header("Aiming Variation (per shot)")]
        [Tooltip("Override jitter % (±). Set <0 to use each turret's own setting.")]
        [Range(-1f, 0.5f)] public float overrideJitterPercent = -1f; // -1 = use turret value
        [Tooltip("Override jitter blend time. Set <0 to use each turret's own setting.")]
        [Range(-1f, 1.5f)] public float overrideJitterDuration = -1f; // -1 = use turret value

        [Header("Testing")]
        public bool testWithFKey = true;
        public bool testUseMouseCell = true;

        private readonly Queue<FireOrder> _toBeFiredAt = new();
        private bool _processing;

        private struct FireOrder
        {
            public BoardView board;
            public GridPos cell;
            public float yOffset;
        }

        void Awake()
        {
            if (enemyBoard == null && BoardController.Instance != null)
                enemyBoard = BoardController.Instance.enemyView;
        }

        void Update()
        {
            if (!testWithFKey) return;
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (enemyBoard == null) return;

                GridPos cell;
                if (testUseMouseCell && TryGetMouseCell(enemyBoard, out cell))
                {
                    EnqueueFireAtCell(enemyBoard, cell, targetYOffset);
                }
                else
                {
                    cell = new GridPos(enemyBoard.width / 2, enemyBoard.height / 2);
                    EnqueueFireAtCell(enemyBoard, cell, targetYOffset);
                }
            }
        }

        // --- Public API ---
        public void EnqueueFireAtCell(BoardView board, GridPos cell, float yOffset = 0f)
        {
            _toBeFiredAt.Enqueue(new FireOrder { board = board, cell = cell, yOffset = yOffset });
            if (!_processing) StartCoroutine(ProcessQueue());
        }

        // Process volleys in order; each volley aims/fires all turrets concurrently
        private IEnumerator ProcessQueue()
        {
            _processing = true;

            while (_toBeFiredAt.Count > 0)
            {
                var order = _toBeFiredAt.Dequeue();
                yield return StartCoroutine(FireVolleyAtCell(order));
            }

            _processing = false;
        }

        private IEnumerator FireVolleyAtCell(FireOrder order)
        {
            int running = 0;
            for (int i = 0; i < turrets.Count; i++)
            {
                var t = turrets[i];
                if (t == null || !t.isActiveAndEnabled) continue;

                running++;
                float delay = basePreFireDelay + perTurretStagger * i;
                StartCoroutine(FireSingleTurret(t, order.board, order.cell, order.yOffset, delay, () => running--));
            }

            if (running == 0) yield return null;
            else yield return new WaitUntil(() => running == 0);
        }

        private IEnumerator FireSingleTurret(GunController turret,
                                             BoardView board,
                                             GridPos cell,
                                             float yOffset,
                                             float delay,
                                             System.Action onDone)
        {
            // Each turret runs independently; it will fire as soon as it's ready.
            yield return StartCoroutine(
                turret.AimAndFireRoutine(
                    board,
                    cell,
                    yOffset,
                    preFireDelay: delay,
                    maxAimTime:   maxAimTime,
                    jitterPct:    (overrideJitterPercent  >= 0f ? overrideJitterPercent  : (float?)null),
                    jitterDur:    (overrideJitterDuration >= 0f ? overrideJitterDuration : (float?)null)
                )
            );
            onDone?.Invoke();
        }

        // --- Helpers ---
        private static bool TryGetMouseCell(BoardView board, out GridPos cell)
        {
            cell = default;

            var cam = Camera.main;
            if (cam == null) return false;

            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 1000f))
                return false;

            return board.WorldToGrid(hit.point, out cell);
        }
    }
}
