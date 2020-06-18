using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [HideInInspector] public static ArenaManager instance;

    public int pillarCount;
    public int startTitanCount;

    [SerializeField] private bool spawnPillars = true;
    [SerializeField] private bool spawnTitans = true;

    public int currentWave;

    public List<TitanController> titansAlive;

    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject titanPrefab;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentWave = 0;

        if (spawnPillars)
        {
            SpawnPillars();
        }
        if (spawnTitans)
        {
            SpawnTitans(0);
        }
    }

    private void Update()
    {
        if (titansAlive.Count <= 0 && spawnTitans)
        {
            SpawnTitans(currentWave);
        }
    }

    private void SpawnPillars()
    {
        for (int i = 0; i <= pillarCount; i++)
        {
            var t = Instantiate(towerPrefab, getGeneratedPosition(), Quaternion.identity);
            t.transform.SetParent(transform);
        }
    }

    private void SpawnTitans(int count)
    {
        for (int i = 0; i < startTitanCount + count; i++)
        {
            var t = Instantiate(titanPrefab, getGeneratedPosition(), Quaternion.identity);
            t.transform.SetParent(transform);

            var controller = t.GetComponent<TitanController>();
            titansAlive.Add(controller);
            controller.OnDeath += Titan_OnDeath;
        }
    }

    private void Titan_OnDeath(object sender, System.EventArgs e)
    {
        titansAlive.Remove((TitanController)sender);
    }

    private Vector3 getGeneratedPosition()
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