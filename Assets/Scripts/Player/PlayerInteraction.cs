using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
  [Header("Interaction Settings")]
  [SerializeField] private float interactionRange = 2f;
  [SerializeField] private Transform cameraTransform;
  [SerializeField] private TextMeshProUGUI interactionText;

  private void Update()
  {
    HandleInteraction();
  }

  private void HandleInteraction()
  {
    RaycastHit hit;
    Vector3 rayOrigin = cameraTransform.position;
    Vector3 rayDirection = cameraTransform.forward;

    Debug.DrawRay(rayOrigin, rayDirection * interactionRange, Color.red, 0.5f);

    if (Physics.Raycast(rayOrigin, rayDirection, out hit, interactionRange))
    {
      ProcessHit(hit);
    }
    else
    {
      ClearInteractionText();
    }
  }

  private void ProcessHit(RaycastHit hit)
  {
    if (hit.collider.CompareTag("Interactable"))
    {
      IInteractableObject interactable = hit.collider.GetComponent<IInteractableObject>();
      if (interactable != null)
      {
        interactionText.text = interactable.GetInteractionMessage();

        if (Input.GetKeyDown(KeyCode.E))
        {
          interactable.Interact();
        }
      }
    }
    else
    {
      ClearInteractionText();
    }
  }

  private void ClearInteractionText()
  {
    if (interactionText != null)
    {
      interactionText.text = string.Empty;
    }
  }
}
