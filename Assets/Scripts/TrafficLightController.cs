using System.Collections;
using UnityEngine;

/// <summary>
/// Traffic Light Controller for pedestrian crossings
/// Manages the stop/go cycle with realistic timing
/// </summary>
public class TrafficLightController : MonoBehaviour
{
    [Header("Traffic Light Settings")]
    [SerializeField] private float stopDuration = 30f; // Red light duration
    [SerializeField] private float goDuration = 15f;   // Green light duration
    [SerializeField] private float warningDuration = 5f; // Yellow/orange light duration
    
    [Header("Light Objects")]
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject yellowLight;
    [SerializeField] private GameObject greenLight;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip stopSound;
    [SerializeField] private AudioClip tickingSound;
    
    [Header("Visual Indicators")]
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material offMaterial;
    
    public enum TrafficLightState
    {
        Stop,    // Red - Don't walk
        Warning, // Yellow/Orange - Prepare to stop
        Go       // Green - Safe to walk
    }
    
    [SerializeField] private TrafficLightState currentState = TrafficLightState.Stop;
    private Coroutine lightCycleCoroutine;
    
    // Events for other systems to subscribe to
    public System.Action<TrafficLightState> OnStateChanged;
    
    public TrafficLightState CurrentState => currentState;
    
    private void Start()
    {
        // Initialize light materials if not set
        SetupMaterials();
        
        // Start the traffic light cycle
        StartLightCycle();
    }
    
    /// <summary>
    /// Sets up default materials if not assigned
    /// </summary>
    private void SetupMaterials()
    {
        if (redMaterial == null)
        {
            redMaterial = CreateColorMaterial(Color.red);
        }
        if (yellowMaterial == null)
        {
            yellowMaterial = CreateColorMaterial(Color.yellow);
        }
        if (greenMaterial == null)
        {
            greenMaterial = CreateColorMaterial(Color.green);
        }
        if (offMaterial == null)
        {
            offMaterial = CreateColorMaterial(Color.gray);
        }
    }
    
    /// <summary>
    /// Creates a material with specified color
    /// </summary>
    private Material CreateColorMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 0.5f);
        return mat;
    }
    
    /// <summary>
    /// Starts the traffic light cycle
    /// </summary>
    public void StartLightCycle()
    {
        if (lightCycleCoroutine != null)
        {
            StopCoroutine(lightCycleCoroutine);
        }
        lightCycleCoroutine = StartCoroutine(TrafficLightCycle());
    }
    
    /// <summary>
    /// Stops the traffic light cycle
    /// </summary>
    public void StopLightCycle()
    {
        if (lightCycleCoroutine != null)
        {
            StopCoroutine(lightCycleCoroutine);
            lightCycleCoroutine = null;
        }
    }
    
    /// <summary>
    /// Main traffic light cycle coroutine
    /// </summary>
    private IEnumerator TrafficLightCycle()
    {
        while (true)
        {
            // Stop phase (Red light)
            SetState(TrafficLightState.Stop);
            yield return new WaitForSeconds(stopDuration);
            
            // Go phase (Green light)
            SetState(TrafficLightState.Go);
            yield return new WaitForSeconds(goDuration);
            
            // Warning phase (Yellow light)
            SetState(TrafficLightState.Warning);
            yield return new WaitForSeconds(warningDuration);
        }
    }
    
    /// <summary>
    /// Sets the traffic light state and updates visuals
    /// </summary>
    private void SetState(TrafficLightState newState)
    {
        currentState = newState;
        UpdateLightVisuals();
        PlayStateSound();
        
        // Notify subscribers
        OnStateChanged?.Invoke(currentState);
        
        Debug.Log($"Traffic Light: {currentState}");
    }
    
    /// <summary>
    /// Updates the visual appearance of the traffic lights
    /// </summary>
    private void UpdateLightVisuals()
    {
        // Reset all lights to off
        if (redLight != null)
            SetLightMaterial(redLight, offMaterial);
        if (yellowLight != null)
            SetLightMaterial(yellowLight, offMaterial);
        if (greenLight != null)
            SetLightMaterial(greenLight, offMaterial);
        
        // Set active light based on state
        switch (currentState)
        {
            case TrafficLightState.Stop:
                if (redLight != null)
                    SetLightMaterial(redLight, redMaterial);
                break;
                
            case TrafficLightState.Warning:
                if (yellowLight != null)
                    SetLightMaterial(yellowLight, yellowMaterial);
                break;
                
            case TrafficLightState.Go:
                if (greenLight != null)
                    SetLightMaterial(greenLight, greenMaterial);
                break;
        }
    }
    
    /// <summary>
    /// Sets the material of a light object
    /// </summary>
    private void SetLightMaterial(GameObject lightObject, Material material)
    {
        Renderer renderer = lightObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }
    
    /// <summary>
    /// Plays audio for the current state
    /// </summary>
    private void PlayStateSound()
    {
        if (audioSource == null) return;
        
        switch (currentState)
        {
            case TrafficLightState.Stop:
                if (stopSound != null)
                    audioSource.PlayOneShot(stopSound);
                break;
                
            case TrafficLightState.Go:
                if (walkSound != null)
                    audioSource.PlayOneShot(walkSound);
                // Also play ticking sound for countdown
                if (tickingSound != null)
                    StartCoroutine(PlayTickingSound());
                break;
                
            case TrafficLightState.Warning:
                // Faster ticking for warning
                if (tickingSound != null)
                    StartCoroutine(PlayTickingSound(0.5f));
                break;
        }
    }
    
    /// <summary>
    /// Plays ticking sound at specified intervals
    /// </summary>
    private IEnumerator PlayTickingSound(float interval = 1f)
    {
        TrafficLightState startState = currentState;
        while (currentState == startState && audioSource != null && tickingSound != null)
        {
            audioSource.PlayOneShot(tickingSound);
            yield return new WaitForSeconds(interval);
        }
    }
    
    /// <summary>
    /// Gets time remaining in current state
    /// </summary>
    public float GetTimeRemainingInState()
    {
        // This would require tracking the start time of each state
        // For now, return a placeholder value
        switch (currentState)
        {
            case TrafficLightState.Stop:
                return stopDuration;
            case TrafficLightState.Go:
                return goDuration;
            case TrafficLightState.Warning:
                return warningDuration;
            default:
                return 0f;
        }
    }
    
    /// <summary>
    /// Manually triggers state change (for testing)
    /// </summary>
    [System.Obsolete("This method is for testing only")]
    public void ManuallySetState(TrafficLightState state)
    {
        StopLightCycle();
        SetState(state);
    }
    
    /// <summary>
    /// Checks if it's safe to cross
    /// </summary>
    public bool IsSafeToCross()
    {
        return currentState == TrafficLightState.Go;
    }
}