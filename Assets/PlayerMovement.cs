using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public Transform playerCamera;
    public Transform handTransform;  // Assign your hand transform here
    public float mouseSensitivity = 150f;
    public float moveSpeed = 12f;
    public float sprintSpeed = 7.5f;  // Speed while sprinting
    public float gravity = -9.81f;
    public GameObject doorPrompt;  // Assign the prompt GameObject here in Inspector
    public GameObject axePrompt;  // Assign the axe prompt GameObject here in Inspector
    
    CharacterController characterController;
    float yaw, pitch;
    float verticalVelocity;
    private GameObject heldAxe;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null && Camera.main)
            playerCamera = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = 0f;
    }

    void Update()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (playerCamera)
            playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // Check if sprinting
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        
        Vector3 move = (transform.right * h + transform.forward * v).normalized * currentSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Raycast always active
        PerformRaycast();
    }

    void PerformRaycast()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main != null ? Camera.main.ScreenPointToRay(screenCenter) : new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;
        float raycastDistance = 100f;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Door interaction: press E while aiming at the door
            if (hit.collider.CompareTag("door"))
            {
                Debug.Log("Door detected: " + hit.collider.gameObject.name);
                if (doorPrompt != null)
                    doorPrompt.SetActive(true);
                if (axePrompt != null)
                    axePrompt.SetActive(false);
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    var door = hit.collider.GetComponent<doorOpen>();
                    if (door != null)
                        door.ToggleDoor();
                }

                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
            // Axe pickup
            else if (hit.collider.CompareTag("Axe"))
            {
                Debug.Log("Axe detected!");
                if (doorPrompt != null)
                    doorPrompt.SetActive(false);
                if (axePrompt != null)
                    axePrompt.SetActive(true);
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    heldAxe = hit.collider.gameObject;
                    heldAxe.transform.parent = handTransform;
                    heldAxe.transform.localPosition = Vector3.zero;
                    heldAxe.transform.localRotation = Quaternion.Euler(0, -90, 90);
                    
                    Rigidbody rb = heldAxe.GetComponent<Rigidbody>();
                    if (rb != null)
                        rb.isKinematic = true;

                    if (axePrompt != null)
                        axePrompt.SetActive(false);
                    
                    Debug.Log("Picked up: " + heldAxe.name);
                }

                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
            else
            {
                if (doorPrompt != null)
                    doorPrompt.SetActive(false);
                if (axePrompt != null)
                    axePrompt.SetActive(false);
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
        }
        else
        {
            if (doorPrompt != null)
                doorPrompt.SetActive(false);
            if (axePrompt != null)
                axePrompt.SetActive(false);
        }
    }
}