using UnityEngine;

[RequireComponent(typeof(CharacterController))]

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float gravity = -9.8f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform; // Transform da câmera
    public Vector3 cameraOffset; // Offset da câmera

    [Header("Head Bobbing")]
    public bool enableHeadBobbing = true;
    public float bobFrequency = 1.5f; // Frequência do balanço
    public float bobAmplitude = 0.05f; // Intensidade do balanço
    public float sprintBobMultiplier = 1.5f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private float defaultCameraYPosition;
    private float bobTimer = 0f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraTransform == null)
        {
            Debug.LogWarning("PlayerController: Câmera não atribuída. Certifique-se de atribuir a Transform da câmera!");
        }

        // Trava o cursor no centro da tela
        Cursor.lockState = CursorLockMode.Locked;

        // Salva a posição inicial da câmera com offset
        defaultCameraYPosition = cameraTransform.localPosition.y + cameraOffset.y;
        cameraTransform.localPosition += cameraOffset;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        ApplyHeadBobbing();
    }

    private void HandleMouseLook()
    {
      float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
      float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

      // Rotação vertical (câmera)
      xRotation -= mouseY;
      xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limita a rotação vertical

      yRotation += mouseX;

      // Aplica a rotação na câmera
      cameraTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

      // Atualiza a posição da câmera para seguir a rotação do jogador
      cameraTransform.position = transform.position + cameraOffset;

    }

    private void HandleMovement()
    {
      float moveX = Input.GetAxis("Horizontal");
      float moveZ = Input.GetAxis("Vertical");

      // Direção de movimento baseada na rotação da câmera
      Vector3 forward = cameraTransform.forward;
      Vector3 right = cameraTransform.right;

      // Normaliza os vetores para evitar movimento diagonal mais rápido
      forward.y = 0f;
      right.y = 0f;
      forward.Normalize();
      right.Normalize();

      Vector3 inputDirection = forward * moveZ + right * moveX;

      // Aplica velocidade de movimento
      float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;
      moveDirection = inputDirection * currentSpeed;

      // Aplica gravidade
      moveDirection.y += gravity * Time.deltaTime;

      // Move o jogador
      characterController.Move(moveDirection * Time.deltaTime);
    }

    private void ApplyHeadBobbing()
    {
        if (!enableHeadBobbing || !characterController.isGrounded || moveDirection.magnitude <= 0.1f)
        {
            // Reseta a posição da câmera quando o jogador está parado
            bobTimer = 0f;
            Vector3 resetPosition = cameraTransform.localPosition;
            resetPosition.y = Mathf.Lerp(cameraTransform.localPosition.y, defaultCameraYPosition, Time.deltaTime * 8f);
            cameraTransform.localPosition = resetPosition;
            return;
        }

        // Calcula a frequência do balanço com base no movimento (corrida aumenta a frequência)
        float bobFrequencyAdjusted = bobFrequency;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            bobFrequencyAdjusted *= sprintBobMultiplier;
        }

        // Atualiza o timer de balanço
        bobTimer += Time.deltaTime * bobFrequencyAdjusted;

        // Calcula o deslocamento da câmera (movimento senoide)
        float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;

        // Aplica o balanço na posição da câmera
        Vector3 newCameraPosition = cameraTransform.localPosition;
        newCameraPosition.y = defaultCameraYPosition + bobOffset;
        cameraTransform.localPosition = newCameraPosition;
    }
}
