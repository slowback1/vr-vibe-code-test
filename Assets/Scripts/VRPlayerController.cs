using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR Player Controller for street crossing training
/// Handles forward/backward movement and head tracking
/// </summary>
public class VRPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float stepSize = 0.5f;
    
    [Header("VR Components")]
    [SerializeField] private XRRig xrRig;
    [SerializeField] private ActionBasedController leftController;
    [SerializeField] private ActionBasedController rightController;
    
    [Header("Movement Controls")]
    [SerializeField] private GameObject forwardButton;
    [SerializeField] private GameObject backwardButton;
    
    [Header("Safety Settings")]
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float raycastDistance = 2f;
    
    private Vector3 targetPosition;
    private bool isMoving = false;
    private TrafficLightController trafficLight;
    
    private void Start()
    {
        // Find the XR Rig if not assigned
        if (xrRig == null)
            xrRig = FindObjectOfType<XRRig>();
            
        // Find traffic light controller
        trafficLight = FindObjectOfType<TrafficLightController>();
        
        // Set initial target position
        targetPosition = transform.position;
        
        // Setup button interactions
        SetupMovementButtons();
    }
    
    private void Update()
    {
        HandleMovement();
    }
    
    /// <summary>
    /// Sets up the VR button interactions for movement
    /// </summary>
    private void SetupMovementButtons()
    {
        if (forwardButton != null)
        {
            var forwardInteractable = forwardButton.GetComponent<XRSimpleInteractable>();
            if (forwardInteractable != null)
            {
                forwardInteractable.selectEntered.AddListener(OnForwardButtonPressed);
            }
        }
        
        if (backwardButton != null)
        {
            var backwardInteractable = backwardButton.GetComponent<XRSimpleInteractable>();
            if (backwardInteractable != null)
            {
                backwardInteractable.selectEntered.AddListener(OnBackwardButtonPressed);
            }
        }
    }
    
    /// <summary>
    /// Handles forward button press
    /// </summary>
    public void OnForwardButtonPressed(SelectEnterEventArgs args)
    {
        MoveForward();
    }
    
    /// <summary>
    /// Handles backward button press
    /// </summary>
    public void OnBackwardButtonPressed(SelectEnterEventArgs args)
    {
        MoveBackward();
    }
    
    /// <summary>
    /// Moves player forward by step size
    /// </summary>
    public void MoveForward()
    {
        if (isMoving) return;
        
        Vector3 forward = transform.forward;
        Vector3 newPosition = transform.position + forward * stepSize;
        
        // Check if movement is safe
        if (IsMovementSafe(newPosition))
        {
            targetPosition = newPosition;
            isMoving = true;
        }
    }
    
    /// <summary>
    /// Moves player backward by step size
    /// </summary>
    public void MoveBackward()
    {
        if (isMoving) return;
        
        Vector3 backward = -transform.forward;
        Vector3 newPosition = transform.position + backward * stepSize;
        
        // Check if movement is safe
        if (IsMovementSafe(newPosition))
        {
            targetPosition = newPosition;
            isMoving = true;
        }
    }
    
    /// <summary>
    /// Smoothly handles movement to target position
    /// </summary>
    private void HandleMovement()
    {
        if (!isMoving) return;
        
        // Smooth movement
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
        
        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }
    
    /// <summary>
    /// Checks if movement to new position is safe
    /// </summary>
    private bool IsMovementSafe(Vector3 newPosition)
    {
        // Raycast to check for ground
        Ray ray = new Ray(newPosition + Vector3.up, Vector3.down);
        bool hasGround = Physics.Raycast(ray, raycastDistance, groundLayer);
        
        // Check for obstacles
        bool hasObstacle = Physics.CheckSphere(newPosition, 0.3f);
        
        return hasGround && !hasObstacle;
    }
    
    /// <summary>
    /// Gets the current head position for looking around
    /// </summary>
    public Vector3 GetHeadPosition()
    {
        if (xrRig != null && xrRig.cameraGameObject != null)
        {
            return xrRig.cameraGameObject.transform.position;
        }
        return transform.position + Vector3.up * 1.7f; // Default head height
    }
    
    /// <summary>
    /// Gets the current head rotation for looking direction
    /// </summary>
    public Quaternion GetHeadRotation()
    {
        if (xrRig != null && xrRig.cameraGameObject != null)
        {
            return xrRig.cameraGameObject.transform.rotation;
        }
        return transform.rotation;
    }
    
    /// <summary>
    /// Checks if player is in crossing zone
    /// </summary>
    public bool IsInCrossingZone()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("CrossingZone"))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Called when player enters a trigger zone
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CrossingZone"))
        {
            Debug.Log("Player entered crossing zone");
        }
        else if (other.CompareTag("SafeZone"))
        {
            Debug.Log("Player entered safe zone");
        }
    }
    
    /// <summary>
    /// Called when player exits a trigger zone
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CrossingZone"))
        {
            Debug.Log("Player left crossing zone");
        }
    }
}