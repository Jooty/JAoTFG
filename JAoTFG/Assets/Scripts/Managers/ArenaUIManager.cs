using TMPro;
using UnityEngine;

public class ArenaUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    // locals
    private ArenaManager arena;

    private void Start()
    {
        arena = GetComponent<ArenaManager>();
    }

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        text.text = $"Wave: {arena.currentWave + 1} | Titans Alive: {arena.titansAlive.Count}";
    }
}