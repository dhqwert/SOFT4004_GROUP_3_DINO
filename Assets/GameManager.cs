using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool gameOver;
    public static bool levelWin;

    public GameObject gameOverPannal;
    public GameObject levelWinPannal;

    private void Start()
    {
        gameOver = false;
        levelWin = false;
        Time.timeScale = 1; 
    }

    private void Update () {
        if(gameOver) {
            Time.timeScale = 0; 
            gameOverPannal.SetActive (true); 
            
            if(Input.GetMouseButtonDown(0)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if(levelWin) {
            levelWinPannal.SetActive (true); 
            
            if(Input.GetMouseButtonDown (0)) {
                LevelManager.instance.PassLevelAndLoadNext();
            }
        }
    }
}