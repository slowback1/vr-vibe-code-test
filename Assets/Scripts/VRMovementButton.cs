using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR Movement Button that can be pressed to trigger player movement
/// Provides visual and haptic feedback when pressed
/// </summary>
public class VRMovementButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private MovementDirection direction = MovementDirection.Forward;
    [SerializeField] private float pressDepth = 0.1f;
    [SerializeField] private float pressSpeed = 5f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color pressedColor = Color.yellow;
    [SerializeField] private Color hoverColor = Color.cyan;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private AudioClip releaseSound;
    
    public enum MovementDirection
    {
        Forward,
        Backward
    }
    
    private XRSimpleInteractable interactable;
    private Renderer buttonRenderer;
    private Material buttonMaterial;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private bool isHovered = false;
    
    private VRPlayerController playerController;
    
    private void Start()
    {
        // Get components
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonMaterial = buttonRenderer.material;
        }
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find player controller
        playerController = FindObjectOfType<VRPlayerController>();
        
        // Store original position
        originalPosition = transform.localPosition;
        pressedPosition = originalPosition - Vector3.up * pressDepth;
        
        // Setup interaction events
        SetupInteractionEvents();
        
        // Set initial color
        UpdateButtonColor(normalColor);
    }
    
    private void Update()
    {
        // Smooth button movement
        Vector3 targetPosition = isPressed ? pressedPosition : originalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, pressSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Sets up the XR interaction events
    /// </summary>
    private void SetupInteractionEvents()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnButtonPressed);
            interactable.selectExited.AddListener(OnButtonReleased);
            interactable.hoverEntered.AddListener(OnButtonHoverEnter);
            interactable.hoverExited.AddListener(OnButtonHoverExit);
        }
    }
    
    /// <summary>
    /// Called when button is pressed
    /// </summary>
    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        isPressed = true;
        UpdateButtonColor(pressedColor);
        PlayPressSound();
        
        // Trigger haptic feedback
        TriggerHapticFeedback(args.interactorObject);
        
        // Trigger movement
        TriggerMovement();
        
        Debug.Log($"{direction} button pressed");
    }
    
    /// <summary>
    /// Called when button is released
    /// </summary>
    private void OnButtonReleased(SelectExitEventArgs args)
    {
        isPressed = false;
        UpdateButtonColor(isHovered ? hoverColor : normalColor);
        PlayReleaseSound();
        
        Debug.Log($"{direction} button released");
    }
    
    /// <summary>
    /// Called when controller hovers over button
    /// </summary>
    private void OnButtonHoverEnter(HoverEnterEventArgs args)
    {
        isHovered = true;
        if (!isPressed)
        {
            UpdateButtonColor(hoverColor);
        }
        
        Debug.Log($"{direction} button hover enter");
    }
    
    /// <summary>
    /// Called when controller stops hovering over button
    /// </summary>
    private void OnButtonHoverExit(HoverExitEventArgs args)
    {
        isHovered = false;
        if (!isPressed)
        {
            UpdateButtonColor(normalColor);
        }
        
        Debug.Log($"{direction} button hover exit");
    }
    
    /// <summary>
    /// Updates the button color
    /// </summary>
    private void UpdateButtonColor(Color color)
    {
        if (buttonMaterial != null)
        {
            buttonMaterial.color = color;
            
            // Add emission for better visibility
            buttonMaterial.EnableKeyword("_EMISSION");
            buttonMaterial.SetColor("_EmissionColor", color * 0.3f);
        }
    }
    
    /// <summary>
    /// Plays button press sound
    /// </summary>
    private void PlayPressSound()
    {
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }
    }
    
    /// <summary>
    /// Plays button release sound
    /// </summary>
    private void PlayReleaseSound()
    {
        if (audioSource != null && releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }
    }
    
    /// <summary>
    /// Triggers haptic feedback on the controller
    /// </summary>
    private void TriggerHapticFeedback(IXRInteractor interactor)
    {
        if (interactor is XRBaseController controller)
        {
            controller.SendHapticImpulse(0.5f, 0.1f);
        }
    }
    
    /// <summary>
    /// Triggers the appropriate movement based on button direction
    /// </summary>
    private void TriggerMovement()
    {
        if (playerController == null)
        {
            Debug.LogWarning("No VR Player Controller found!");
            return;
        }
        
        switch (direction)
        {
            case MovementDirection.Forward:
                playerController.MoveForward();
                break;
            case MovementDirection.Backward:
                playerController.MoveBackward();
                break;
        }
    }
    
    /// <summary>
    /// Creates a movement button prefab
    /// </summary>
    public static GameObject CreateMovementButton(MovementDirection direction, Vector3 position, Transform parent = null)
    {
        // Create button object
        GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        button.name = $"VRButton_{direction}";
        button.transform.position = position;
        button.transform.localScale = Vector3.one * 0.2f;
        
        if (parent != null)
        {
            button.transform.parent = parent;
        }
        
        // Add VR Movement Button component
        VRMovementButton buttonComponent = button.AddComponent<VRMovementButton>();
        buttonComponent.direction = direction;
        
        // Set button color based on direction
        Color buttonColor = direction == MovementDirection.Forward ? Color.green : Color.red;
        buttonComponent.normalColor = buttonColor;
        
        // Add collider for interaction
        BoxCollider collider = button.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = false; // XR Interaction Toolkit needs solid colliders
        }
        
        return button;
    }
    
    /// <summary>
    /// Sets the movement direction of this button
    /// </summary>
    public void SetDirection(MovementDirection newDirection)
    {
        direction = newDirection;
        
        // Update color based on direction
        Color newColor = direction == MovementDirection.Forward ? Color.green : Color.red;
        normalColor = newColor;
        
        if (!isPressed && !isHovered)
        {
            UpdateButtonColor(normalColor);
        }
    }
    
    /// <summary>
    /// Gets the current direction of this button
    /// </summary>
    public MovementDirection GetDirection()
    {
        return direction;
    }
}