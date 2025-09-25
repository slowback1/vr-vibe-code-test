using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI controller for vehicles that obey traffic laws
/// Handles stopping at intersections, following traffic lights, and realistic driving behavior
/// </summary>
public class VehicleAI : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private float accelerationRate = 2f;
    [SerializeField] private float decelerationRate = 4f;
    
    [Header("Navigation")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool loopWaypoints = true;
    [SerializeField] private NavMeshAgent navMeshAgent;
    
    [Header("Traffic Behavior")]
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private LayerMask vehicleLayer = 1 << 8;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float followDistance = 3f;
    
    [Header("Traffic Light Detection")]
    [SerializeField] private float trafficLightDetectionRange = 15f;
    
    private int currentWaypointIndex = 0;
    private float currentSpeed = 0f;
    private bool isStopped = false;
    private TrafficLightController nearbyTrafficLight;
    private VehicleAI vehicleInFront;
    
    // Vehicle state
    private enum VehicleState
    {
        Driving,
        StoppedAtLight,
        StoppedForVehicle,
        StoppedForObstacle
    }
    
    private VehicleState currentState = VehicleState.Driving;
    
    private void Start()
    {
        // Setup NavMesh agent
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();
            
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = normalSpeed;
            navMeshAgent.stoppingDistance = 0.5f;
        }
        
        // Find the first waypoint
        if (waypoints.Length > 0)
        {
            SetDestination(waypoints[0].position);
        }
        
        // Start the traffic detection coroutine
        StartCoroutine(TrafficDetectionLoop());
    }
    
    private void Update()
    {
        HandleMovement();
        CheckWaypointReached();
        UpdateVehicleState();
    }
    
    /// <summary>
    /// Handles vehicle movement and speed control
    /// </summary>
    private void HandleMovement()
    {
        if (navMeshAgent == null) return;
        
        float targetSpeed = GetTargetSpeed();
        
        // Smooth speed transitions
        if (currentSpeed < targetSpeed)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);
        }
        else if (currentSpeed > targetSpeed)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, decelerationRate * Time.deltaTime);
        }
        
        navMeshAgent.speed = currentSpeed;
        
        // Update stopped state
        isStopped = currentSpeed < 0.1f;
    }
    
    /// <summary>
    /// Determines the target speed based on current conditions
    /// </summary>
    private float GetTargetSpeed()
    {
        // Check for stop conditions
        if (ShouldStopForTrafficLight() || ShouldStopForVehicle() || ShouldStopForObstacle())
        {
            return 0f;
        }
        
        // Check for following vehicle (maintain distance)
        if (vehicleInFront != null)
        {
            float distanceToVehicle = Vector3.Distance(transform.position, vehicleInFront.transform.position);
            if (distanceToVehicle < followDistance * 2f)
            {
                // Slow down to match vehicle in front
                return Mathf.Clamp(vehicleInFront.currentSpeed, 0f, normalSpeed * 0.7f);
            }
        }
        
        return normalSpeed;
    }
    
    /// <summary>
    /// Checks if vehicle should stop for traffic light
    /// </summary>
    private bool ShouldStopForTrafficLight()
    {
        if (nearbyTrafficLight == null) return false;
        
        float distanceToLight = Vector3.Distance(transform.position, nearbyTrafficLight.transform.position);
        
        // If close to traffic light and it's not green
        if (distanceToLight <= stoppingDistance && !nearbyTrafficLight.IsSafeToCross())
        {
            // Check if we're approaching the light (not moving away)
            Vector3 directionToLight = (nearbyTrafficLight.transform.position - transform.position).normalized;
            Vector3 vehicleForward = transform.forward;
            
            if (Vector3.Dot(directionToLight, vehicleForward) > 0.5f)
            {
                currentState = VehicleState.StoppedAtLight;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if vehicle should stop for another vehicle
    /// </summary>
    private bool ShouldStopForVehicle()
    {
        vehicleInFront = DetectVehicleInFront();
        
        if (vehicleInFront != null)
        {
            float distanceToVehicle = Vector3.Distance(transform.position, vehicleInFront.transform.position);
            if (distanceToVehicle <= followDistance)
            {
                currentState = VehicleState.StoppedForVehicle;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if vehicle should stop for obstacles
    /// </summary>
    private bool ShouldStopForObstacle()
    {
        // Raycast forward to detect obstacles
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, stoppingDistance, obstacleLayer))
        {
            // Ignore other vehicles (handled separately)
            if (!hit.collider.CompareTag("Vehicle"))
            {
                currentState = VehicleState.StoppedForObstacle;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Detects if there's a vehicle in front
    /// </summary>
    private VehicleAI DetectVehicleInFront()
    {
        // Raycast forward to detect vehicles
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, detectionRange, vehicleLayer))
        {
            VehicleAI otherVehicle = hit.collider.GetComponent<VehicleAI>();
            if (otherVehicle != null)
            {
                return otherVehicle;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Traffic detection loop running as a coroutine
    /// </summary>
    private IEnumerator TrafficDetectionLoop()
    {
        while (true)
        {
            DetectNearbyTrafficLight();
            yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds
        }
    }
    
    /// <summary>
    /// Detects nearby traffic lights
    /// </summary>
    private void DetectNearbyTrafficLight()
    {
        TrafficLightController[] trafficLights = FindObjectsOfType<TrafficLightController>();
        TrafficLightController closestLight = null;
        float closestDistance = float.MaxValue;
        
        foreach (var light in trafficLights)
        {
            float distance = Vector3.Distance(transform.position, light.transform.position);
            if (distance < trafficLightDetectionRange && distance < closestDistance)
            {
                // Check if the light is in front of the vehicle
                Vector3 directionToLight = (light.transform.position - transform.position).normalized;
                Vector3 vehicleForward = transform.forward;
                
                if (Vector3.Dot(directionToLight, vehicleForward) > 0.3f) // 70-degree cone in front
                {
                    closestLight = light;
                    closestDistance = distance;
                }
            }
        }
        
        nearbyTrafficLight = closestLight;
    }
    
    /// <summary>
    /// Updates the vehicle state for debugging
    /// </summary>
    private void UpdateVehicleState()
    {
        if (!isStopped)
        {
            currentState = VehicleState.Driving;
        }
    }
    
    /// <summary>
    /// Checks if vehicle has reached current waypoint
    /// </summary>
    private void CheckWaypointReached()
    {
        if (waypoints.Length == 0 || navMeshAgent == null) return;
        
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            // Move to next waypoint
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopWaypoints)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    // Reached end of waypoints
                    return;
                }
            }
            
            SetDestination(waypoints[currentWaypointIndex].position);
        }
    }
    
    /// <summary>
    /// Sets the destination for navigation
    /// </summary>
    private void SetDestination(Vector3 destination)
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(destination);
        }
    }
    
    /// <summary>
    /// Gets the current speed of the vehicle
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    /// <summary>
    /// Gets the current state of the vehicle
    /// </summary>
    public string GetCurrentState()
    {
        return currentState.ToString();
    }
    
    /// <summary>
    /// Manually stops the vehicle (for emergencies)
    /// </summary>
    public void EmergencyStop()
    {
        currentSpeed = 0f;
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
    }
    
    /// <summary>
    /// Resumes vehicle movement after emergency stop
    /// </summary>
    public void ResumeMovement()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
        }
    }
    
    // Gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw stopping distance
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * stoppingDistance);
        
        // Draw waypoint path
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
            
            // Draw loop connection
            if (loopWaypoints && waypoints[waypoints.Length - 1] != null && waypoints[0] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
            }
        }
    }
}