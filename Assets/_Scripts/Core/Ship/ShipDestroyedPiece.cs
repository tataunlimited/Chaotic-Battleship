using DG.Tweening;
using UnityEngine;

public class ShipDestroyedPiece : MonoBehaviour
{


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Grid"))
        {
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Collider>().enabled = false;
            transform.DOMoveY(-1f, 2f).SetDelay(Random.Range(0.2f,.6f));

        }
    }
}
