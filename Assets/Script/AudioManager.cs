using UnityEngine;
using System;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;

    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Sounds")]
    public Sound[] sounds;

    [Header("Nhạc nền (Kéo AudioSource nhạc nền vào đây để tắt khi vào game)")]
    public AudioSource bgmSource;

    void Awake()
    {
        // Singleton: Chỉ cho phép 1 AudioManager tồn tại xuyên suốt game
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tắt nhạc nền khi vào game
        if (bgmSource != null)
        {
            bgmSource.Stop();
            Debug.Log("Đã tắt nhạc nền!");
        }

        // Tự động tạo AudioSource cho mỗi Sound đã khai báo
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // Gọi hàm này để phát âm thanh: AudioManager.instance.Play("GameOver");
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Không tìm thấy âm thanh tên: " + name);
            return;
        }
        // Cập nhật lại volume/pitch phòng trường hợp chỉnh trong Inspector lúc runtime
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.Play();
    }

    // Dừng phát âm thanh
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Không tìm thấy âm thanh tên: " + name);
            return;
        }
        s.source.Stop();
    }

    // Kiểm tra xem âm thanh có đang phát không
    public bool IsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return false;
        return s.source.isPlaying;
    }
}
