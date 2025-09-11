using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Ship
{
    public class ShipMovementComponent : MonoBehaviour
    {

        private NavMeshAgent _navMeshAgent;
        private NavMeshObstacle _navMeshObstacle;
        public Rigidbody rigidbody;
        Quaternion _rotation;
        Vector3 _position;
        private bool _isMoving;
        void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshObstacle = GetComponent<NavMeshObstacle>();
            _navMeshAgent.updateRotation = false;
            rigidbody = GetComponent<Rigidbody>();
            
        }

        // Update is called once per frame
        void Update()
        {
            if (_navMeshAgent.enabled)
            {
               // Debug.Log("RemainingDistance: "+_navMeshAgent.remainingDistance + " StoppingDistance: " + _navMeshAgent.stoppingDistance);
            }
            if(_navMeshAgent.enabled && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && _isMoving)
            {
                //_rigidbody.isKinematic = true;
                _isMoving = false;
                //transform.DORotateQuaternion(_rotation, 1f).SetDelay(2);
                transform.DORotateQuaternion(_rotation, 1f);
                UpdateAllShipPositions();
            }
           
        }

        void UpdateAllShipPositions()
        {
            Debug.Log("Updating all ship positions from: " + gameObject.name);
            var components = FindObjectsByType<ShipMovementComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var component in components)
            {
                if(component == this)
                    continue;

                component.CorrectPosition();
            }

        }

        private void CorrectPosition()
        { 
            Debug.Log("Correcting position for: " + gameObject.name);
            rigidbody.isKinematic = true;
            _navMeshAgent.enabled = false;
            _navMeshObstacle.enabled = true;
           transform.DOMove(_position, 0.5f);
           //transform.DORotateQuaternion(_rotation, 0.5f);
        }
        public void MoveToPosition(Vector3 position, Quaternion rotation)
        {
            var components = FindObjectsByType<ShipMovementComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if(_navMeshAgent == null)
                _navMeshAgent = GetComponent<NavMeshAgent>();
            foreach (var component in components)
            {
                if (component == this)
                {
                    component.GetComponent<NavMeshObstacle>().enabled = false;
                    component.GetComponent<NavMeshAgent>().enabled = true;
                    rigidbody.isKinematic = true;
                    
                    continue;
                }
                component.GetComponent<NavMeshObstacle>().enabled = true;
                component.GetComponent<NavMeshAgent>().enabled = false;
                component.rigidbody.isKinematic = false;

            }

            _navMeshAgent.SetDestination(position);
            _rotation = rotation;
            
            _isMoving = true;
            _position = position;
           

        }
    }
}
