using UnityEngine;

[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerInteraction))]
public class PlayerController : MonoBehaviour
{
    [Header("Moviment Settings")] 
    public float moveSpeed = 5f;

    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 15f;
    public float gravity = -9.81f;
    public Transform CameraTransform;
    
    private CharacterController controller;
    private Vector3 playerVelocity;
    
    private PlayerStatus playerStatus;
    private PlayerInteraction playerInteraction;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerStatus = GetComponent<PlayerStatus>();
        playerInteraction = GetComponent<PlayerInteraction>();
        
        if (CameraTransform == null)
        {
            Debug.LogWarning("PlayerController: A câmera não está atribuída. Certifique-se de atribuir a Transform da câmera!");
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Movimentação no plano horizontal
        Vector3 inputDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Aplica velocidade de movimento
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;
            playerVelocity = inputDirection * currentSpeed;

            // Alinha a direção do movimento à direção da câmera
            playerVelocity = Quaternion.Euler(0f, CameraTransform.eulerAngles.y, 0f) * playerVelocity;
        }
        
        // Aplica gravidade
        playerVelocity.y += gravity * Time.deltaTime;

        // Move o jogador
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // Rotaticiona o jogador com base na camera
        Vector3 forwardDirection = CameraTransform.forward;
        forwardDirection.y = 0f;
        
        Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Rotaciona a câmera
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        Vector3 cameraRotation = CameraTransform.eulerAngles;
        cameraRotation.x -= mouseY;
        cameraRotation.y += mouseX;
        
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -90f, 90f);
        
        CameraTransform.eulerAngles = cameraRotation;
    }
}