using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GmaeOverUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public Button restartbutton;
    public Button quitbutton;

    private GameObject setting;

    void Start()
    {
        setting = transform.GetChild(0)?.gameObject;

        restartbutton.onClick.AddListener(OnGameRestart);
        quitbutton.onClick.AddListener(OnGameQuit);
        scoreText.text = GameManager.Instance.score.ToString();
    }

    public void OnGameRestart()
    {
        GameManager.Instance.movable = 21;
        GameManager.Instance.target = 5;
        GameManager.Instance.score = 0;
        SceneManager.LoadScene(0);
    }

    public void OnGameQuit()
    {
        Application.Quit();
    }
}
