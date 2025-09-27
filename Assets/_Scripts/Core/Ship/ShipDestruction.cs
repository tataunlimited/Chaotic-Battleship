using Unity.VisualScripting;
using UnityEngine;

namespace Core.Ship
{
    public class ShipDestruction : MonoBehaviour
    {
        [Tooltip("The minimum (X) and maximum (Y) force of the explosion.")]
        public Vector2 explosionForceRange = new Vector2(2, 8);
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            var shipView = GetComponentInParent<ShipView>();

            if (shipView.shipModel.type == ShipType.Submarine)
            {
                foreach (var r in renderers)
                {
                    r.AddComponent<MeshRenderer>();
                    r.AddComponent<Rigidbody>();
                }
                return;
            }
            
            foreach (var r in renderers)
            {
                r.AddComponent<MeshRenderer>();
                var rb = r.AddComponent<Rigidbody>();

                if (r.gameObject.name.ToLower().Contains("surface"))
                    continue;
                Vector3 randomDirection = Random.onUnitSphere;
                randomDirection.y = Mathf.Abs(randomDirection.y);

                // Apply the force in that random upward direction
                
                float randomExplosionForce = Random.Range(explosionForceRange.x, explosionForceRange.y);

                rb.AddForce(randomDirection * randomExplosionForce, ForceMode.Impulse);

            }
        }


    }
}
