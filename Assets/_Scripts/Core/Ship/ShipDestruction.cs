using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Core.Ship
{
    public class ShipDestruction : MonoBehaviour
    {
        [Tooltip("The minimum (X) and maximum (Y) force of the explosion.")]
        public Vector2 explosionForceRange = new Vector2(2, 8);
        
        public float upwardBias = 2f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var shipView = GetComponentInParent<ShipView>();
            if (shipView.shipModel.type == ShipType.Submarine)
            {
                return;
            }
            
            var renderers = GetComponentsInChildren<Renderer>();

            foreach (var r in renderers)
            {

                if (r.gameObject.name.ToLower().Contains("surface"))
                {
                    r.AddComponent<MeshCollider>();

                    r.transform.DOMoveY(-1f, 2f).SetDelay(Random.Range(0.5f,1f));
                    r.transform.DORotate(new Vector3(Random.Range(-45,45), 0), 2f);
                    continue;
                }
                var rb = r.AddComponent<Rigidbody>();
                r.AddComponent<BoxCollider>();
                r.AddComponent<ShipDestroyedPiece>();

                Vector3 xzDirection = Random.onUnitSphere;
                Vector3 randomDirection = new Vector3(xzDirection.x, upwardBias, xzDirection.y);

                randomDirection.Normalize(); 
                randomDirection.y = Mathf.Abs(randomDirection.y);

                // Apply the force in that random upward direction
                
                float randomExplosionForce = Random.Range(explosionForceRange.x, explosionForceRange.y);

                rb.AddForce(randomDirection * randomExplosionForce, ForceMode.Impulse);

            }
        }


    }
}
