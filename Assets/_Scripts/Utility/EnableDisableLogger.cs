using UnityEngine;

namespace Utility
{
    public class EnableDisableLogger : MonoBehaviour
    {
        void OnEnable()
        {
            Debug.Log($"Enabled : {gameObject}");
        }
    
        void OnDisable()
        {
            Debug.Log($"Disabled : {gameObject}");
        }
    }
}
