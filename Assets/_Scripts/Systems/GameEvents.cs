using Core.Ship;
using System;

public static class GameEvents
{
    public static event Action<ShipType> OnPlayerHitSegment;
    public static event Action<ShipType> OnPlayerDestroyedShip;
    public static event Action OnWaveCleared;

    public static void RaiseHitSegment(ShipType type) => OnPlayerHitSegment?.Invoke(type);
    public static void RaiseDestroyedShip(ShipType type) => OnPlayerDestroyedShip?.Invoke(type);
    public static void RaiseWaveCleared() => OnWaveCleared?.Invoke();
}
