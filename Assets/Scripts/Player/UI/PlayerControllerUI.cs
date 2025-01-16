
  using System.Runtime.CompilerServices;
  using UnityEngine;

  [RequireComponent(typeof(PlayerInventoryUI))]
  public class PlayerControllerUI : MonoBehaviour
  {
      private PlayerInventoryUI playerInventoryUI;

      private void Start()
      {
          playerInventoryUI = GetComponent<PlayerInventoryUI>();
      }

      private void Update()
      {
          if (Input.GetKeyDown(KeyCode.I))
          {
              playerInventoryUI.ToggleInventory();
          }
      }
  }
