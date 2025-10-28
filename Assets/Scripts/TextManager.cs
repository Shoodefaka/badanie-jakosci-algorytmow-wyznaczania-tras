using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    public int score;
    public Text scoreText;
    public Text timerText;
    float elapsedTime = 0;
    public bool startSimulation = false;
    Grid grid;

    void Awake() {
        grid = GetComponent<Grid>();
    }

    void Update() {
        if (startSimulation) {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void AddScore() {
        score++;
        scoreText.text = "Wynik: " + score.ToString();
    }

    public void ResetScoreAndTime() {
        score = 0;
        scoreText.text = "Wynik: " + score.ToString();
        elapsedTime = 0;
        timerText.text = string.Format("{0:00}:{1:00}", 0, 0);
        grid.stopScore = 0;
    }
}
