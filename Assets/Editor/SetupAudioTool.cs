using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SetupAudioTool
{
    [MenuItem("Tools/4. Cài Đặt Âm Thanh (Coin & Powerup)")]
    public static void SetupAudio()
    {
        // Yêu cầu Unity load file âm thanh vừa copy
        AssetDatabase.Refresh();

        string coinSoundPath = "Assets/Audios/CoinSound.wav";
        string powerupSoundPath = "Assets/Audios/PowerupSound.wav";

        AudioClip coinClip = AssetDatabase.LoadAssetAtPath<AudioClip>(coinSoundPath);
        AudioClip powerupClip = AssetDatabase.LoadAssetAtPath<AudioClip>(powerupSoundPath);

        if (coinClip == null || powerupClip == null)
        {
            Debug.LogError("Chưa tìm thấy file âm thanh. Bạn vui lòng thử ấn lại nút này nhé!");
            return;
        }

        // Tìm AudioManager trong Scene, nếu không có thì tự tạo mới!
        AudioManager audioManager = GameObject.FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            GameObject amObj = new GameObject("AudioManager");
            audioManager = amObj.AddComponent<AudioManager>();
            Debug.Log("Không tìm thấy AudioManager cũ, hệ thống đã tự động tạo mới một AudioManager cho bạn!");
        }

        Undo.RecordObject(audioManager, "Cập nhật âm thanh");

        List<Sound> soundList = new List<Sound>();
        if (audioManager.sounds != null)
        {
            soundList = audioManager.sounds.ToList();
        }

        // Thêm hoặc cập nhật âm thanh "Coin"
        Sound coinSound = soundList.Find(s => s.name == "Coin");
        if (coinSound == null)
        {
            coinSound = new Sound { name = "Coin", volume = 1f, pitch = 1f };
            soundList.Add(coinSound);
        }
        coinSound.clip = coinClip;

        // Thêm hoặc cập nhật âm thanh "Powerup"
        Sound powerupSound = soundList.Find(s => s.name == "Powerup");
        if (powerupSound == null)
        {
            powerupSound = new Sound { name = "Powerup", volume = 1f, pitch = 1f };
            soundList.Add(powerupSound);
        }
        powerupSound.clip = powerupClip;

        audioManager.sounds = soundList.ToArray();

        PrefabUtility.RecordPrefabInstancePropertyModifications(audioManager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(audioManager.gameObject.scene);

        Debug.Log("THÀNH CÔNG! Đã nạp âm thanh nhặt Coin và Xuyên Phá vào hệ thống.");
    }
}
