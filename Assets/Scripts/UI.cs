 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI movableText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI settingScoreText;
    public Button settingButton;
    public Button restartbutton;
    public Button quitbutton;

    private GameObject setting;
    private bool setBool;

    void Start()
    {
        setting = transform.GetChild(3)?.gameObject;

        setting.SetActive(false);

        settingButton.onClick.AddListener(OnSetting);
        restartbutton.onClick.AddListener(OnGameRestart);
        quitbutton.onClick.AddListener(OnGameQuit);
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
        settingScoreText.text = score.ToString();
    }

    public void SetTarget(int target)
    {
        targetText.text = target.ToString();
    }

    public void SetMovable(int movable)
    {
        movableText.text = movable.ToString();
    }

    public void OnSetting()
    {
        setting.SetActive(!setBool);
        setBool = !setBool;
    }

    public void OnGameRestart()
    {
        GameManager.Instance.movable = 21;
        GameManager.Instance.target = 5;
        GameManager.Instance.score = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnGameQuit()
    {
        Application.Quit();
    }
}
