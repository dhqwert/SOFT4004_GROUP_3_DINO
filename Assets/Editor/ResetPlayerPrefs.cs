using UnityEngine;
using UnityEditor;

public class ResetPlayerPrefs
{
    [MenuItem("Tools/Reset PlayerPrefs (Xóa Save Game)")]
    public static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Đã xóa toàn bộ PlayerPrefs! Level và dữ liệu đã được reset về mặc định.");
    }
}
