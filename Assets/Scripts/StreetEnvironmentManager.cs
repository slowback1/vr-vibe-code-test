using UnityEngine;

/// <summary>
/// Street Environment Manager
/// Manages the overall street crossing environment including roads, sidewalks, and crossings
/// </summary>
public class StreetEnvironmentManager : MonoBehaviour
{
    [Header("Environment Prefabs")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject sidewalkPrefab;
    [SerializeField] private GameObject crosswalkPrefab;
    [SerializeField] private GameObject buildingPrefab;
    
    [Header("Street Configuration")]
    [SerializeField] private float streetWidth = 8f;
    [SerializeField] private float sidewalkWidth = 3f;
    [SerializeField] private float crosswalkWidth = 4f;
    [SerializeField] private float intersectionSize = 12f;
    
    [Header("Traffic Light Positions")]
    [SerializeField] private Vector3[] trafficLightPositions = new Vector3[]
    {
        new Vector3(6, 3, 0),   // East side
        new Vector3(-6, 3, 0),  // West side
        new Vector3(0, 3, 6),   // North side
        new Vector3(0, 3, -6)   // South side
    };
    
    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform[] vehicleSpawnPoints;
    
    [Header("Materials")]
    [SerializeField] private Material roadMaterial;
    [SerializeField] private Material sidewalkMaterial;
    [SerializeField] private Material crosswalkMaterial;
    
    private void Start()
    {
        GenerateStreetEnvironment();
    }
    
    /// <summary>
    /// Generates the complete street environment
    /// </summary>
    public void GenerateStreetEnvironment()
    {
        CreateRoads();
        CreateSidewalks();
        CreateCrosswalks();
        CreateTrafficLights();
        CreateBuildings();
        SetupCollisionZones();
        
        Debug.Log("Street environment generated successfully");
    }
    
    /// <summary>
    /// Creates the road surfaces
    /// </summary>
    private void CreateRoads()
    {
        // Main road (East-West)
        CreateRoadSegment(Vector3.zero, new Vector3(50, 0.1f, streetWidth), roadMaterial, "MainRoad_EW");
        
        // Cross road (North-South)
        CreateRoadSegment(Vector3.zero, new Vector3(streetWidth, 0.1f, 50), roadMaterial, "MainRoad_NS");
        
        // Intersection
        CreateRoadSegment(Vector3.zero, new Vector3(intersectionSize, 0.1f, intersectionSize), roadMaterial, "Intersection");
    }
    
    /// <summary>
    /// Creates sidewalks along the roads
    /// </summary>
    private void CreateSidewalks()
    {
        float sidewalkOffset = (streetWidth + sidewalkWidth) / 2f;
        
        // North sidewalks
        CreateSidewalkSegment(new Vector3(0, 0.2f, sidewalkOffset), new Vector3(50, 0.2f, sidewalkWidth), "Sidewalk_North");
        CreateSidewalkSegment(new Vector3(sidewalkOffset, 0.2f, 0), new Vector3(sidewalkWidth, 0.2f, 50), "Sidewalk_East");
        
        // South sidewalks
        CreateSidewalkSegment(new Vector3(0, 0.2f, -sidewalkOffset), new Vector3(50, 0.2f, sidewalkWidth), "Sidewalk_South");
        CreateSidewalkSegment(new Vector3(-sidewalkOffset, 0.2f, 0), new Vector3(sidewalkWidth, 0.2f, 50), "Sidewalk_West");
    }
    
    /// <summary>
    /// Creates pedestrian crosswalks
    /// </summary>
    private void CreateCrosswalks()
    {
        // East-West crosswalks
        CreateCrosswalkSegment(new Vector3(0, 0.15f, (streetWidth/2) + 1), new Vector3(crosswalkWidth, 0.05f, 2), "Crosswalk_North");
        CreateCrosswalkSegment(new Vector3(0, 0.15f, -(streetWidth/2) - 1), new Vector3(crosswalkWidth, 0.05f, 2), "Crosswalk_South");
        
        // North-South crosswalks
        CreateCrosswalkSegment(new Vector3((streetWidth/2) + 1, 0.15f, 0), new Vector3(2, 0.05f, crosswalkWidth), "Crosswalk_East");
        CreateCrosswalkSegment(new Vector3(-(streetWidth/2) - 1, 0.15f, 0), new Vector3(2, 0.05f, crosswalkWidth), "Crosswalk_West");
    }
    
    /// <summary>
    /// Creates traffic lights at intersection
    /// </summary>
    private void CreateTrafficLights()
    {
        GameObject trafficLightPrefab = CreateTrafficLightPrefab();
        
        foreach (Vector3 position in trafficLightPositions)
        {
            GameObject trafficLight = Instantiate(trafficLightPrefab, position, Quaternion.identity);
            trafficLight.name = $"TrafficLight_{position}";
            trafficLight.transform.parent = transform;
        }
    }
    
    /// <summary>
    /// Creates simple buildings for environment
    /// </summary>
    private void CreateBuildings()
    {
        float buildingOffset = (streetWidth + sidewalkWidth + 5) / 2f;
        
        // Corner buildings
        CreateBuilding(new Vector3(buildingOffset, 2.5f, buildingOffset), "Building_NE");
        CreateBuilding(new Vector3(-buildingOffset, 2.5f, buildingOffset), "Building_NW");
        CreateBuilding(new Vector3(buildingOffset, 2.5f, -buildingOffset), "Building_SE");
        CreateBuilding(new Vector3(-buildingOffset, 2.5f, -buildingOffset), "Building_SW");
    }
    
    /// <summary>
    /// Sets up collision and trigger zones
    /// </summary>
    private void SetupCollisionZones()
    {
        // Create crossing detection zones
        CreateCrossingZone(new Vector3(0, 0.5f, (streetWidth/2) + 1), new Vector3(crosswalkWidth, 1, 2));
        CreateCrossingZone(new Vector3(0, 0.5f, -(streetWidth/2) - 1), new Vector3(crosswalkWidth, 1, 2));
        CreateCrossingZone(new Vector3((streetWidth/2) + 1, 0.5f, 0), new Vector3(2, 1, crosswalkWidth));
        CreateCrossingZone(new Vector3(-(streetWidth/2) - 1, 0.5f, 0), new Vector3(2, 1, crosswalkWidth));
        
        // Create safe zones on sidewalks
        CreateSafeZone(new Vector3(0, 0.5f, (streetWidth + sidewalkWidth) / 2f), new Vector3(10, 1, sidewalkWidth));
        CreateSafeZone(new Vector3(0, 0.5f, -(streetWidth + sidewalkWidth) / 2f), new Vector3(10, 1, sidewalkWidth));
    }
    
    /// <summary>
    /// Creates a road segment with specified parameters
    /// </summary>
    private void CreateRoadSegment(Vector3 position, Vector3 size, Material material, string name)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.transform.position = position;
        road.transform.localScale = size;
        road.name = name;
        road.transform.parent = transform;
        
        if (material != null)
        {
            road.GetComponent<Renderer>().material = material;
        }
        
        // Add collider
        road.GetComponent<Collider>().isTrigger = false;
        road.layer = LayerMask.NameToLayer("Ground");
    }
    
    /// <summary>
    /// Creates a sidewalk segment
    /// </summary>
    private void CreateSidewalkSegment(Vector3 position, Vector3 size, string name)
    {
        GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sidewalk.transform.position = position;
        sidewalk.transform.localScale = size;
        sidewalk.name = name;
        sidewalk.transform.parent = transform;
        
        if (sidewalkMaterial != null)
        {
            sidewalk.GetComponent<Renderer>().material = sidewalkMaterial;
        }
        
        sidewalk.layer = LayerMask.NameToLayer("Ground");
    }
    
    /// <summary>
    /// Creates a crosswalk segment
    /// </summary>
    private void CreateCrosswalkSegment(Vector3 position, Vector3 size, string name)
    {
        GameObject crosswalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crosswalk.transform.position = position;
        crosswalk.transform.localScale = size;
        crosswalk.name = name;
        crosswalk.transform.parent = transform;
        
        if (crosswalkMaterial != null)
        {
            crosswalk.GetComponent<Renderer>().material = crosswalkMaterial;
        }
        
        crosswalk.layer = LayerMask.NameToLayer("Ground");
    }
    
    /// <summary>
    /// Creates a traffic light prefab
    /// </summary>
    private GameObject CreateTrafficLightPrefab()
    {
        GameObject trafficLight = new GameObject("TrafficLight");
        
        // Add the controller component
        trafficLight.AddComponent<TrafficLightController>();
        
        // Create light objects
        GameObject redLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        redLight.transform.parent = trafficLight.transform;
        redLight.transform.localPosition = Vector3.up * 0.5f;
        redLight.transform.localScale = Vector3.one * 0.3f;
        redLight.name = "RedLight";
        
        GameObject yellowLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        yellowLight.transform.parent = trafficLight.transform;
        yellowLight.transform.localPosition = Vector3.zero;
        yellowLight.transform.localScale = Vector3.one * 0.3f;
        yellowLight.name = "YellowLight";
        
        GameObject greenLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        greenLight.transform.parent = trafficLight.transform;
        greenLight.transform.localPosition = Vector3.down * 0.5f;
        greenLight.transform.localScale = Vector3.one * 0.3f;
        greenLight.name = "GreenLight";
        
        // Create pole
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.parent = trafficLight.transform;
        pole.transform.localPosition = Vector3.down * 1.5f;
        pole.transform.localScale = new Vector3(0.1f, 1.5f, 0.1f);
        pole.name = "Pole";
        
        return trafficLight;
    }
    
    /// <summary>
    /// Creates a simple building
    /// </summary>
    private void CreateBuilding(Vector3 position, string name)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.transform.position = position;
        building.transform.localScale = new Vector3(8, 5, 8);
        building.name = name;
        building.transform.parent = transform;
        
        // Set building material to gray
        Material buildingMat = new Material(Shader.Find("Standard"));
        buildingMat.color = Color.gray;
        building.GetComponent<Renderer>().material = buildingMat;
    }
    
    /// <summary>
    /// Creates a crossing detection zone
    /// </summary>
    private void CreateCrossingZone(Vector3 position, Vector3 size)
    {
        GameObject zone = new GameObject("CrossingZone");
        zone.transform.position = position;
        zone.transform.parent = transform;
        zone.tag = "CrossingZone";
        
        BoxCollider collider = zone.AddComponent<BoxCollider>();
        collider.size = size;
        collider.isTrigger = true;
    }
    
    /// <summary>
    /// Creates a safe zone
    /// </summary>
    private void CreateSafeZone(Vector3 position, Vector3 size)
    {
        GameObject zone = new GameObject("SafeZone");
        zone.transform.position = position;
        zone.transform.parent = transform;
        zone.tag = "SafeZone";
        
        BoxCollider collider = zone.AddComponent<BoxCollider>();
        collider.size = size;
        collider.isTrigger = true;
    }
}