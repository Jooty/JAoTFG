using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [HideInInspector] public static ArenaManager instance;

    [SerializeField] private bool spawnPillars = true;
    [SerializeField] private bool spawnTitans = true;
    public int pillarCount;
    public int startTitanCount;

    public int currentWave;

    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject titanPrefab;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentWave = 0;
        //titansAlive = new List<Titan>();

        if (spawnPillars)
        {
            SpawnPillars();
        }
    }

    private void SpawnPillars()
    {
        for (int i = 0; i <= pillarCount; i++)
        {
            var t = Instantiate(towerPrefab, GeneratedPosition, Quaternion.identity);
            t.transform.SetParent(this.transform);
        }
    }

    private Vector3 GeneratedPosition
    {
        get
        {
            var size = Terrain.activeTerrain.terrainData.size;
            float x, z;
            x = Random.Range(0, size.x);
            z = Random.Range(0, size.z);
            Vector3 t = new Vector3(x, 0, z);
            float y = Terrain.activeTerrain.SampleHeight(t);
            t = new Vector3(x, y, z);
            return t;
        }
    }
}