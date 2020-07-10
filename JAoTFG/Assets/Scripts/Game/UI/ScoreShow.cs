using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreShow : MonoBehaviour
{

    public TextMeshProUGUI speedText, scoreText;

    // local components
    private Animator anim;

    private void Awake()
    {
        this.anim = GetComponent<Animator>();
    }

    public void ShowScore(DeathInfo info)
    {
        speedText.text = $"Speed: {info.sourceSpeed}";
        scoreText.text = info.score.ToString();

        anim.SetTrigger("appear");
    }

}
