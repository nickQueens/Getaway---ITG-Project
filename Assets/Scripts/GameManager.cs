using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private float timeElapsed;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject player;
    private bool levelComplete = false;
    private int minScore = 3000;
    // Start is called before the first frame update
    void Start()
    {
        timeElapsed = 0;
        scoreText.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        if (!levelComplete)
        {
            timeElapsed += Time.deltaTime;
            timeText.text = "Time: " + (System.Math.Round(timeElapsed, 2)).ToString();
        }

        if (player.transform.position.z > 18)
        {
            levelComplete = true;
            int score = (int)(minScore + Mathf.Round(10000*Mathf.Max((120 - timeElapsed)/120, 0)));
            scoreText.text = "Level complete!<br>Score: " + score.ToString();
        }
    }
}
