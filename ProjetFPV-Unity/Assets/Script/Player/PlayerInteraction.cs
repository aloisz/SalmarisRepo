using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 10f;
    
    // Update is called once per frame
    void Update()
    {
        CheckInteraction();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckInteraction()
    {
        if (PlayerInputs.Instance.isReceivingInteractInputs.performed)
        {
            var nearestColliders = Physics.OverlapSphere(transform.position, interactionRadius);
            var colliderListInterface = new List<Collider>();
            
            foreach (Collider c in nearestColliders)
            {
                IInteract interact = c.gameObject.GetComponent<IInteract>();
                if(interact is not null) colliderListInterface.Add(c);
            }
            
            if (colliderListInterface.Count > 0)
            {
                var orderedEnumerable = colliderListInterface.OrderBy(c => Vector3.Distance(c.transform.position, transform.position)).ToArray();
                Debug.Log(orderedEnumerable[0]);
                
                
                var actor = orderedEnumerable[0].GetComponent<IInteract>();
                actor.Interact();
                
                Debug.Log($"Interacted with {actor}");
            }
        }
    }
}
