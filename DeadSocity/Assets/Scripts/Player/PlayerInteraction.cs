using TMPro;
using UnityEngine;
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")] 
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private TMP_Text interactionText;


    private void HandleInteraction()
    {
        RaycastHit hit;
        Vector3 rayOrigin = interactionPoint.position;
        Vector3 rayDirection = interactionPoint.forward;
        
        Debug.DrawRay(rayOrigin, rayDirection * interactionRange, Color.red, 0.5f);
        
        if(Physics.Raycast(rayOrigin, rayDirection, out hit, interactionRange, interactionLayer))
        {
            ProcessHit(hit);
        }
        else
        {
            interactionText.text = "";
        }
    }

    private void ProcessHit(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent(out IInteractable interactable))
        {
            interactionText.text = interactable.GetInteractionText();
            if (Input.GetKeyDown(KeyCode.E))
            {
                interactable.Interact();
            }
        }
        else
        {
            interactionText.text = "";
        }
    }

}
