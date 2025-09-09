using UnityEngine;

public class PlayerData : MonoBehaviour
{

    public int waveNumber = 1;
    public int currentScore = 0;
    
    
    public static PlayerData Instance;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        
    }
    
}
