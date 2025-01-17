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

    // Desenha o Raycast na cena para depuração
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
    // Verifica se o objeto possui a tag "Interactable"
    if (hit.collider.CompareTag("Interactable"))
    {
      IInteractableObject interactable = hit.collider.GetComponent<IInteractableObject>();
      if (interactable != null)
      {
        interactionText.text = interactable.GetInteractionMessage(); // Atualiza a mensagem na UI

        // Executa a interação quando a tecla E é pressionada
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
