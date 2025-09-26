using Core.Ship;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ShipInfoSlotManager : MonoBehaviour
{
    public string shipName;
    public ShipView tiedShip;
    public Image shipImage;
    public Image healthSlot1;
    public Image healthSlot2;
    public Image healthSlot3;
    public Image healthSlot4;
    public Image armorSlot1;
    public Image armorSlot2;
    public Image armorSlot3;
    public Image armorSlot4;
    public Image armorSlot5;
    public Image armorSlot6;
    public Image armorSlot7;
    public Image armorSlot8;
    public Sprite submarineSprite;
    public Sprite destroyerSprite;
    public Sprite cruiserSprite;
    public Sprite battleshipSprite;
    public Material lostHealthMaterial;
    public Material lostArmorMaterial;
    public TextMeshProUGUI shipNameText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (tiedShip == null) return;
        CheckArmorChanged();
        CheckHealthChanged();
    }

    //for any health change, call this to update the health slots
    public void CheckHealthChanged()
    {
        //the health slots should have been set correctly in UpdateHealthSlots, so just change the material of lost health slots
        int health = tiedShip.shipModel.hp;
        if (health < 4) healthSlot4.material = lostHealthMaterial;
        if (health < 3) healthSlot3.material = lostHealthMaterial;
        if (health < 2) healthSlot2.material = lostHealthMaterial;
        if (health < 1) healthSlot1.material = lostHealthMaterial;
    }

    //for any armor change, call this to update the armor slots
    public void CheckArmorChanged()
    {
        //the armor slots should have been set correctly in UpdateArmorSlots, so just change the material of lost armor slots
        float armor = tiedShip.shipModel.armor;
        if (armor < 4f) armorSlot8.material = lostArmorMaterial;
        if (armor < 3.5f) armorSlot7.material = lostArmorMaterial;
        if (armor < 3f) armorSlot6.material = lostArmorMaterial;
        if (armor < 2.5f) armorSlot5.material = lostArmorMaterial;
        if (armor < 2f) armorSlot4.material = lostArmorMaterial;
        if (armor < 1.5f) armorSlot3.material = lostArmorMaterial;
        if (armor < 1f) armorSlot2.material = lostArmorMaterial;
        if (armor < 0.5f) armorSlot1.material = lostArmorMaterial;
    }

    public void SetShip(ShipView ship)
    {
        tiedShip = ship;
        shipName = ship.shipModel.type.ToString();
        UpdateHealthSlots();
        UpdateArmorSlots();
        //set ship image based on ship type
        switch (tiedShip.shipModel.type)
        {
            case ShipType.Submarine:
                shipImage.sprite = submarineSprite;
                shipName = "Submarine: " + GetRandomHistoricalAmericanShipName(ShipType.Submarine);
                break;
            case ShipType.Destroyer:
                shipImage.sprite = destroyerSprite;
                shipName = "Destroyer: " + GetRandomHistoricalAmericanShipName(ShipType.Destroyer);
                break;
            case ShipType.Cruiser:
                shipImage.sprite = cruiserSprite;
                shipName = "Cruiser: " + GetRandomHistoricalAmericanShipName(ShipType.Cruiser);
                break;
            case ShipType.Battleship:
                shipImage.sprite = battleshipSprite;
                shipName = "Battleship: " + GetRandomHistoricalAmericanShipName(ShipType.Battleship);
                break;
            default:
                Debug.LogError("Unknown ship type: " + tiedShip.shipModel.type);
                break;
        }
        //set the ship name
        shipNameText.text = shipName;
    }

    public void UpdateHealthSlots()
    {
        if (tiedShip == null) return;

        int health = tiedShip.shipModel.hp;
        healthSlot1.gameObject.SetActive(health >= 1);
        healthSlot2.gameObject.SetActive(health >= 2);
        healthSlot3.gameObject.SetActive(health >= 3);
        healthSlot4.gameObject.SetActive(health >= 4);
    }

    public void UpdateArmorSlots()
    {
        if (tiedShip == null) return;
        float armor = tiedShip.shipModel.armor;
        armorSlot1.gameObject.SetActive(armor >= 0.5f);
        armorSlot2.gameObject.SetActive(armor >= 1f);
        armorSlot3.gameObject.SetActive(armor >= 1.5f);
        armorSlot4.gameObject.SetActive(armor >= 2f);
        armorSlot5.gameObject.SetActive(armor >= 2.5f);
        armorSlot6.gameObject.SetActive(armor >= 3f);
        armorSlot7.gameObject.SetActive(armor >= 3.5f);
        armorSlot8.gameObject.SetActive(armor >= 4f);

    }

    public void ClearShip()
    {
        tiedShip = null;
        shipName = "Empty";
        shipImage.sprite = null;
        healthSlot1.gameObject.SetActive(false);
        healthSlot2.gameObject.SetActive(false);
        healthSlot3.gameObject.SetActive(false);
        healthSlot4.gameObject.SetActive(false);
        armorSlot1.gameObject.SetActive(false);
        armorSlot2.gameObject.SetActive(false);
        armorSlot3.gameObject.SetActive(false);
        armorSlot4.gameObject.SetActive(false);
        armorSlot5.gameObject.SetActive(false);
        armorSlot6.gameObject.SetActive(false);
        armorSlot7.gameObject.SetActive(false);
        armorSlot8.gameObject.SetActive(false);
    }

    //get a random historical american ship name based on ship type
    public string GetRandomHistoricalAmericanShipName(ShipType type)
    {
        // Get a random historical American ship name based on the ship type
        string[] submarineNames = { "USS Nautilus", "USS Seawolf", "USS Gato", "USS Balao", "USS Tang" };
        string[] destroyerNames = { "USS Arleigh Burke", "USS Fletcher", "USS Spruance", "USS Zumwalt", "USS Kidd" };
        string[] cruiserNames = { "USS Ticonderoga", "USS Baltimore", "USS New Orleans", "USS San Francisco", "USS Canberra" };
        string[] battleshipNames = { "USS Iowa", "USS Missouri", "USS Wisconsin", "USS New Jersey", "USS South Dakota" };
        //Get a different set for Japanese ships
        string[] japaneseSubmarineNames = { "I-400", "I-58", "I-19", "I-26", "I-168" };
        string[] japaneseDestroyerNames = { "Fubuki", "Kagero", "Yukikaze", "Shimakaze", "Akizuki" };
        string[] japaneseCruiserNames = { "Mogami", "Takao", "Myoko", "Aoba", "Kuma" };
        string[] japaneseBattleshipNames = { "Yamato", "Musashi", "Nagato", "Kongo", "Fuso" };
        switch (type)
        {
            case ShipType.Submarine:
                if (tiedShip.IsPlayer)
                    return submarineNames[Random.Range(0, submarineNames.Length)];
                else
                    return japaneseSubmarineNames[Random.Range(0, japaneseSubmarineNames.Length)];
            case ShipType.Destroyer:
                if (tiedShip.IsPlayer)
                    return destroyerNames[Random.Range(0, destroyerNames.Length)];
                else
                    return japaneseDestroyerNames[Random.Range(0, japaneseDestroyerNames.Length)];
            case ShipType.Cruiser:
                if (tiedShip.IsPlayer)
                    return cruiserNames[Random.Range(0, cruiserNames.Length)];
                else
                    return japaneseCruiserNames[Random.Range(0, japaneseCruiserNames.Length)];
            case ShipType.Battleship:
                if (tiedShip.IsPlayer)
                    return battleshipNames[Random.Range(0, battleshipNames.Length)];
                else
                    return japaneseBattleshipNames[Random.Range(0, japaneseBattleshipNames.Length)];
            default:
                return "Unknown Ship";
        }
    }
}
