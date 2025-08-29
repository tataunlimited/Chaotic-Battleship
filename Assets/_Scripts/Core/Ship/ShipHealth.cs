using Core.Ship;
using UnityEngine;
using UnityEngine.UI;


public class ShipHealth : MonoBehaviour
{
    
    public int currentHealth;
    public Slider healthSlider;
    public ShipView shipView;

    private void Start()
    {
        shipView = GetComponent<ShipView>();
        healthSlider = SliderManager.instance.GetSlider(shipView.shipModel.type);
        currentHealth = shipView.shipModel.hp;
        healthSlider.value = 1;
    }

    public void TakeDamage(int damage)
    {
        
        

        // update slider
        healthSlider.value = (float) shipView.shipModel.hp / shipView.shipModel.MaxHP;

        if (healthSlider.value <= 0)
        {
            DestroyShip();
        }
        Debug.Log("shipHP drop" + shipView.shipModel.hp);
    }

    private void DestroyShip()
    {
        
    }
}
