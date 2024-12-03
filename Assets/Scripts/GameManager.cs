using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float timeElapsed;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform endZone;
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

        if ((Vector3.Distance(endZone.position, player.transform.position) < 15) && !levelComplete)
        {
            levelComplete = true;
            int score = (int)(minScore + Mathf.Round(10000*Mathf.Max((120 - timeElapsed)/120, 0)));
            scoreText.text = "Level complete!";//<br>Score: " + score.ToString();
        }
    }
}
