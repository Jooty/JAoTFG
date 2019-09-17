using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArenaManager : MonoBehaviour
{

    public static ArenaManager instance;

    public int pillarCount;
    public int startTitanCount;

    public int currentWave;
    [HideInInspector] public List<Titan> titansAlive;

    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject titanPrefab;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentWave = 0;
        titansAlive = new List<Titan>();

        SpawnPillars();
        SpawnTitans();
    }

    private void Update()
    {
        if (titansAlive.Count == 0)
        {
            currentWave++;

            SpawnTitans();
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

    private void SpawnTitans()
    {
        for (int i = 0; i <= startTitanCount + currentWave; i++)
        {
            var t = Instantiate(titanPrefab, GeneratedPosition, Quaternion.identity);
            titansAlive.Add(t.GetComponent<Titan>());
        }
    }

    public void RemoveTitan(Titan titan)
    {
        titansAlive.Remove(titan);
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
