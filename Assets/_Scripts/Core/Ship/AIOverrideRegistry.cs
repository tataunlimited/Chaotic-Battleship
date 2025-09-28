using System.Collections.Generic;
using Core.Ship;

namespace Core.Ship
{
    // Simple, centralized place to stash per-type and per-ship AI overrides for the current wave.
    public static class AIOverrideRegistry
    {
        // Per-type: ShipType -> level
        public static readonly Dictionary<ShipType, int> TypeLevels = new();

        // Per-ship: ShipModel.id -> level
        public static readonly Dictionary<string, int> ShipLevels = new();

        public static void Clear()
        {
            TypeLevels.Clear();
            ShipLevels.Clear();
        }

        public static bool TryGetLevel(ShipView view, out int level)
        {
            level = 0;
            if (view == null || view.shipModel == null) return false;

            // Per-ship takes precedence
            var id = view.shipModel.id;
            if (!string.IsNullOrEmpty(id) && ShipLevels.TryGetValue(id, out level))
                return true;

            // Then per-type
            return TypeLevels.TryGetValue(view.shipModel.type, out level);
        }
    }
}
