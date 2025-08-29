using System;
using Core.Ship;
using UnityEngine;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    public Slider battlship;
    public Slider cruiser;
    public Slider sub;
    public Slider destroyer;
    
    
    public GameObject enemyBattleshipDestroyedUI;
    public GameObject enemyCruiserDestroyedUI;
    public GameObject enemySubDestroyedUI;
    public GameObject enemyDestroyerDestroyedUI;
    public static SliderManager instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Slider GetSlider(ShipType type)
    {
        switch (type)
        {
            case ShipType.Destroyer:
                return destroyer;
                
            case ShipType.Battleship:
                return battlship;
                
            case ShipType.Cruiser:
                return cruiser;
                
            case ShipType.Submarine:
                return sub;
        }

        return null;
    }

    public GameObject GetDestroyerUI(ShipType type)
    {
        switch (type)
        {
            case ShipType.Destroyer:
                return enemyDestroyerDestroyedUI;
            case ShipType.Battleship:
                return enemyBattleshipDestroyedUI;
            case ShipType.Cruiser:
                return enemyCruiserDestroyedUI;
            case ShipType.Submarine:
                return enemySubDestroyedUI;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

}
