using System.Linq;
using Core.Board;
using Core.GridSystem;
using Core.Ship;
using UnityEngine;

public static class BoardStateSerializer
{
    // Capture ships from the live boards
    public static GameState Capture(BoardController board, int wave, string phase)
    {
        return new GameState
        {
            waveNumber = wave,
            phase = phase,
            playerBoard = CaptureBoard(board.playerView),
            enemyBoard  = CaptureBoard(board.enemyView),
        };
    }

    private static BoardState CaptureBoard(BoardView view)
    {
        var bs = new BoardState();

        // Walk all live ships on this board
        foreach (var shipView in view.SpawnedShips.Values)
        {
            var m = shipView.shipModel;
            bs.ships.Add(new ShipState
            {
                type        = m.type,
                length      = m.length,
                rootX       = m.root.x,
                rootY       = m.root.y,
                orientation = m.orientation.ToString(),  // "North"/"East"/"South"/"West"
                hp          = m.hp,
                isSunk      = m.IsSunk
            });
        }

        return bs;
    }

    // Rebuild boards from a saved snapshot
    public static void Apply(BoardController board, GameState state)
    {
        // Clear current boards
        board.playerView.Reset();
        board.enemyView.Reset();

        // Player side
        ApplyBoard(board, board.playerView, state.playerBoard);

        // Enemy side
        ApplyBoard(board, board.enemyView, state.enemyBoard);

        // Refresh visuals/UI
        board.UpdateBoards();
    }

    private static void ApplyBoard(BoardController board, BoardView view, BoardState bs)
    {
        // We need ShipView prefabs to place ships. They are on BoardController.shipPrefabs.
        foreach (var s in bs.ships)
        {
            // Find a prefab of the correct type
            var prefab = board.shipPrefabs.Find(p => p.shipModel.type == s.type);
            if (prefab == null)
            {
                Debug.LogError($"[BoardStateSerializer] Missing prefab for {s.type}");
                continue;
            }

            var pos = new GridPos(s.rootX, s.rootY);
            var ori = ParseOrientation(s.orientation);

            // Place the ship
            if (view.TryPlaceShip(prefab, pos, ori, out var instance))
            {
                // Restore HP after Init() (which resets HP)
                instance.shipModel.hp = Mathf.Clamp(s.hp, 0, instance.shipModel.MaxHP);

                // If you want to mark it as sunk visually, you can do that by reducing hp to 0.
                // The game logic uses IsSunk => isDestroyed || hp <= 0. Setting hp=0 is enough for logic.
                // If you have additional "sunk" visuals, trigger them here.
            }
            else
            {
                Debug.LogWarning($"[BoardStateSerializer] Failed to place {s.type} at {pos} ({ori})");
            }
        }
    }

    private static Orientation ParseOrientation(string str)
    {
        // Your enum is North/East/South/West. Safe parse with fallback.
        if (System.Enum.TryParse<Orientation>(str, out var o)) return o;
        return Orientation.North;
    }
}
