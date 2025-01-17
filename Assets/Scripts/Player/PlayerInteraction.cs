using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
  [Header("Interaction Settings")] public float interactionRange = 2f;
  public TextMeshProUGUI interactionText; // Adicione um campo para o texto de interação
  public Transform cameraTransform;
  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.E)) // Tecla de interação
    {
      RaycastHit hit;
      Vector3 rayOrigin = cameraTransform.position;
      Vector3 rayDirection = cameraTransform.forward;
      Debug.DrawRay(rayOrigin, rayDirection * interactionRange, Color.red, 1f); // Desenha o Raycast na cena

      if (Physics.Raycast(rayOrigin, rayDirection, out hit, interactionRange))
      {
        Debug.Log("Raycast hit: " + hit.collider.name); // Loga o nome do objeto atingido
        if (hit.collider.CompareTag("Interactable"))
        {
          Debug.Log("Hit object: " + hit.collider.name);
          // Procura pelo componente de interação no objeto
          IInteractableObject interactable = hit.collider.GetComponent<IInteractableObject>();
          if (interactable != null)
          {
            interactable.Interact();
            interactionText.text = interactable.GetInteractionMessage(); // Exibe a mensagem de interação
          }
        }
        else
        {
          Debug.Log("Hit object is not interactable");
        }
      }
      else
      {
        Debug.Log("Raycast did not hit any object");
      }
    }
  }
}
