using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRadius = 0.5f;
    [SerializeField] private LayerMask interactMask;


    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactMask);
        if (hits == null || hits.Length == 0) return;

        foreach (var col in hits)
        {
            if (col == null) continue;
            var interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
                return; 
            }

            var parentInteractable = col.GetComponentInParent<IInteractable>();
            if (parentInteractable != null)
            {
                parentInteractable.Interact();
                return;
            }
        }
    }
}
