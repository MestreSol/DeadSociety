using UnityEngine;

[RequireComponent(typeof(PlayerInventoryUI))]
public class PlayerControllerUI : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private PlayerInventoryUI playerInventoryUI;

  private void Awake()
  {
    if (playerInventoryUI == null)
    {
      playerInventoryUI = GetComponent<PlayerInventoryUI>();
    }
  }

  private void Update()
  {
    HandleInventoryToggle();
  }

  private void HandleInventoryToggle()
  {
    if (Input.GetKeyDown(KeyCode.I))
    {
      playerInventoryUI.ToggleInventory();
    }
  }
}
