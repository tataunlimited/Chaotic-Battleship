using Core.Ship;
using UnityEngine;
using UnityEngine.UI;


public class ShipHealth : MonoBehaviour
{
    
    public Slider healthSlider;
    public GameObject destroyedShip;
    public ShipView shipView;

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        shipView = GetComponent<ShipView>();
        if (shipView.IsPlayer)
        {
            healthSlider = SliderManager.instance.GetSlider(shipView.shipModel.type);
            healthSlider.value = 1;
        }
        else
        {
            destroyedShip = SliderManager.instance.GetDestroyerUI(shipView.shipModel.type);
            EnableDestroyedState(false);
        }

        
    }
    public void UpdateHealthBar()
    {
        // update slider
        healthSlider.value = (float) shipView.shipModel.hp / shipView.shipModel.MaxHP;

        if (healthSlider.value <= 0)
        {
            DestroyShip();
        }
        Debug.Log($"shipHP drop {gameObject.name} :: " + shipView.shipModel.hp);
    }

    private void DestroyShip()
    {
        
    }

    public void EnableDestroyedState(bool b)
    {
        if(destroyedShip!=null)
            destroyedShip.SetActive(b);
    }
}
