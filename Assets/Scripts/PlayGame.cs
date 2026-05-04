using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayGame : MonoBehaviour
{
    // Đổi tên hàm thành StartGame để không bị trùng với tên class (PlayGame)
    public void StartGame() {
        // Lấy level hiện tại đã lưu để load đúng Scene đang chơi dở
        int levelToLoad = PlayerPrefs.GetInt("CurrentLevel", 1);
        
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        if (levelToLoad < totalScenes) {
            SceneManager.LoadScene(levelToLoad);
        } else {
            SceneManager.LoadScene(Random.Range(1, totalScenes));
        }
    }
}