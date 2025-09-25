using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR Scene Manager that sets up the complete street crossing training scene
/// Manages all components and their interactions
/// </summary>
public class VRSceneManager : MonoBehaviour
{
    [Header("VR Setup")]
    [SerializeField] private GameObject xrRigPrefab;
    [SerializeField] private Vector3 playerStartPosition = new Vector3(0, 0, -8);
    
    [Header("Environment")]
    [SerializeField] private StreetEnvironmentManager environmentManager;
    
    [Header("Movement Buttons")]
    [SerializeField] private Vector3 forwardButtonOffset = new Vector3(0.5f, 1.2f, 0.3f);
    [SerializeField] private Vector3 backwardButtonOffset = new Vector3(-0.5f, 1.2f, 0.3f);
    
    [Header("Traffic Settings")]
    [SerializeField] private GameObject vehiclePrefab;
    [SerializeField] private int numberOfVehicles = 4;
    
    private GameObject vrPlayer;
    private VRPlayerController playerController;
    private GameObject forwardButton;
    private GameObject backwardButton;
    
    private void Start()
    {
        SetupVRScene();
    }
    
    /// <summary>
    /// Sets up the complete VR scene
    /// </summary>
    private void SetupVRScene()
    {
        Debug.Log("Setting up VR Street Crossing Scene...");
        
        // 1. Setup Environment
        SetupEnvironment();
        
        // 2. Setup VR Player
        SetupVRPlayer();
        
        // 3. Setup Movement Buttons
        SetupMovementButtons();
        
        // 4. Setup Traffic
        SetupTraffic();
        
        // 5. Setup Audio
        SetupAudio();
        
        Debug.Log("VR Scene setup complete!");
    }
    
    /// <summary>
    /// Sets up the street environment
    /// </summary>
    private void SetupEnvironment()
    {
        if (environmentManager == null)
        {
            GameObject envObject = new GameObject("StreetEnvironment");
            environmentManager = envObject.AddComponent<StreetEnvironmentManager>();
        }
        
        environmentManager.GenerateStreetEnvironment();
    }
    
    /// <summary>
    /// Sets up the VR player with XR Rig
    /// </summary>
    private void SetupVRPlayer()
    {
        // Create VR Player object
        vrPlayer = new GameObject("VRPlayer");
        vrPlayer.transform.position = playerStartPosition;
        
        // Add player controller
        playerController = vrPlayer.AddComponent<VRPlayerController>();
        
        // Setup XR Rig (simplified version)
        SetupXRRig();
        
        // Add capsule collider for player
        CapsuleCollider playerCollider = vrPlayer.AddComponent<CapsuleCollider>();
        playerCollider.height = 1.8f;
        playerCollider.center = Vector3.up * 0.9f;
        
        // Add rigidbody
        Rigidbody rb = vrPlayer.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.mass = 70f;
    }
    
    /// <summary>
    /// Sets up a simplified XR Rig
    /// </summary>
    private void SetupXRRig()
    {
        // Create XR Origin (replaces XR Rig in newer versions)
        GameObject xrOrigin = new GameObject("XR Origin");
        xrOrigin.transform.parent = vrPlayer.transform;
        xrOrigin.transform.localPosition = Vector3.zero;
        
        // Create Camera Offset
        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.parent = xrOrigin.transform;
        cameraOffset.transform.localPosition = Vector3.zero;
        
        // Create Main Camera
        GameObject mainCamera = new GameObject("Main Camera (Head)");
        mainCamera.transform.parent = cameraOffset.transform;
        mainCamera.transform.localPosition = new Vector3(0, 1.7f, 0); // Head height
        mainCamera.tag = "MainCamera";
        
        // Add Camera component
        Camera camera = mainCamera.AddComponent<Camera>();
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1000f;
        
        // Add Audio Listener
        mainCamera.AddComponent<AudioListener>();
        
        // Create Controllers
        SetupControllers(cameraOffset);
    }
    
    /// <summary>
    /// Sets up VR controllers
    /// </summary>
    private void SetupControllers(GameObject cameraOffset)
    {
        // Left Controller
        GameObject leftController = new GameObject("Left Controller");
        leftController.transform.parent = cameraOffset.transform;
        leftController.transform.localPosition = new Vector3(-0.3f, 1.0f, 0.2f);
        
        // Right Controller  
        GameObject rightController = new GameObject("Right Controller");
        rightController.transform.parent = cameraOffset.transform;
        rightController.transform.localPosition = new Vector3(0.3f, 1.0f, 0.2f);
        
        // Add controller representations (simple cubes for now)
        CreateControllerRepresentation(leftController, "LeftHand");
        CreateControllerRepresentation(rightController, "RightHand");
    }
    
    /// <summary>
    /// Creates a visual representation for controllers
    /// </summary>
    private void CreateControllerRepresentation(GameObject controller, string name)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = name + "_Visual";
        visual.transform.parent = controller.transform;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.1f, 0.05f, 0.2f);
        
        // Make it look like a controller
        Material controllerMat = new Material(Shader.Find("Standard"));
        controllerMat.color = Color.black;
        visual.GetComponent<Renderer>().material = controllerMat;
    }
    
    /// <summary>
    /// Sets up movement buttons for VR interaction
    /// </summary>
    private void SetupMovementButtons()
    {
        // Create forward button
        forwardButton = VRMovementButton.CreateMovementButton(
            VRMovementButton.MovementDirection.Forward,
            vrPlayer.transform.position + forwardButtonOffset,
            vrPlayer.transform
        );
        
        // Create backward button
        backwardButton = VRMovementButton.CreateMovementButton(
            VRMovementButton.MovementDirection.Backward,
            vrPlayer.transform.position + backwardButtonOffset,
            vrPlayer.transform
        );
        
        // Update player controller references
        if (playerController != null)
        {
            // Set button references in player controller via reflection or public fields
            // This is a simplified approach - in a real project you'd use proper serialization
        }
    }
    
    /// <summary>
    /// Sets up traffic system with vehicles
    /// </summary>
    private void SetupTraffic()
    {
        if (vehiclePrefab == null)
        {
            vehiclePrefab = CreateVehiclePrefab();
        }
        
        // Spawn vehicles at different positions
        Vector3[] spawnPositions = new Vector3[]
        {
            new Vector3(15, 0.5f, 2),   // East road
            new Vector3(-15, 0.5f, -2), // West road
            new Vector3(2, 0.5f, 15),   // North road
            new Vector3(-2, 0.5f, -15)  // South road
        };
        
        for (int i = 0; i < Mathf.Min(numberOfVehicles, spawnPositions.Length); i++)
        {
            GameObject vehicle = Instantiate(vehiclePrefab, spawnPositions[i], Quaternion.identity);
            vehicle.name = $"Vehicle_{i+1}";
            
            // Setup waypoints for this vehicle
            SetupVehicleWaypoints(vehicle, i);
        }
    }
    
    /// <summary>
    /// Creates a simple vehicle prefab
    /// </summary>
    private GameObject CreateVehiclePrefab()
    {
        GameObject vehicle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vehicle.name = "Vehicle";
        vehicle.transform.localScale = new Vector3(2, 1, 4);
        vehicle.tag = "Vehicle";
        
        // Set vehicle color
        Material vehicleMat = new Material(Shader.Find("Standard"));
        vehicleMat.color = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
        vehicle.GetComponent<Renderer>().material = vehicleMat;
        
        // Add Vehicle AI component
        vehicle.AddComponent<VehicleAI>();
        
        // Add NavMesh Agent
        vehicle.AddComponent<UnityEngine.AI.NavMeshAgent>();
        
        // Add Rigidbody
        Rigidbody rb = vehicle.AddComponent<Rigidbody>();
        rb.mass = 1000;
        rb.drag = 2;
        
        return vehicle;
    }
    
    /// <summary>
    /// Sets up waypoints for a vehicle
    /// </summary>
    private void SetupVehicleWaypoints(GameObject vehicle, int vehicleIndex)
    {
        VehicleAI vehicleAI = vehicle.GetComponent<VehicleAI>();
        if (vehicleAI == null) return;
        
        // Create waypoints based on vehicle spawn position
        Transform[] waypoints;
        Vector3 startPos = vehicle.transform.position;
        
        if (Mathf.Abs(startPos.x) > Mathf.Abs(startPos.z)) // East-West road
        {
            waypoints = new Transform[2];
            waypoints[0] = CreateWaypoint(new Vector3(-20, 0, startPos.z), $"Waypoint_{vehicleIndex}_0");
            waypoints[1] = CreateWaypoint(new Vector3(20, 0, startPos.z), $"Waypoint_{vehicleIndex}_1");
        }
        else // North-South road
        {
            waypoints = new Transform[2];
            waypoints[0] = CreateWaypoint(new Vector3(startPos.x, 0, -20), $"Waypoint_{vehicleIndex}_0");
            waypoints[1] = CreateWaypoint(new Vector3(startPos.x, 0, 20), $"Waypoint_{vehicleIndex}_1");
        }
        
        // Assign waypoints to vehicle AI via reflection (simplified approach)
        var waypointsField = typeof(VehicleAI).GetField("waypoints", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (waypointsField != null)
        {
            waypointsField.SetValue(vehicleAI, waypoints);
        }
    }
    
    /// <summary>
    /// Creates a waypoint at specified position
    /// </summary>
    private Transform CreateWaypoint(Vector3 position, string name)
    {
        GameObject waypoint = new GameObject(name);
        waypoint.transform.position = position;
        return waypoint.transform;
    }
    
    /// <summary>
    /// Sets up ambient audio for the scene
    /// </summary>
    private void SetupAudio()
    {
        // Create ambient audio source
        GameObject audioObject = new GameObject("AmbientAudio");
        AudioSource ambientSource = audioObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.volume = 0.3f;
        ambientSource.spatialBlend = 0; // 2D audio
        
        // You would assign ambient city sounds here
        // ambientSource.clip = ambientCitySound;
        // ambientSource.Play();
    }
    
    /// <summary>
    /// Gets the VR player controller
    /// </summary>
    public VRPlayerController GetPlayerController()
    {
        return playerController;
    }
    
    /// <summary>
    /// Gets the environment manager
    /// </summary>
    public StreetEnvironmentManager GetEnvironmentManager()
    {
        return environmentManager;
    }
}