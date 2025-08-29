using Core.Ship;
using UnityEngine;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    public Slider battlship;
    public Slider cruiser;
    public Slider sub;
    public Slider destroyer;
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

}
