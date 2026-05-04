using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdaptiveUIRuntime : MonoBehaviour
{
    private static bool _initialized;

    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyAfterSceneLoad()
    {
        ApplyAll();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAll();
    }

    private static void ApplyAll()
    {
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>(true);
        for (int i = 0; i < scalers.Length; i++)
        {
            ConfigureScaler(scalers[i]);
        }

        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null)
            {
                continue;
            }

            if (canvas.rootCanvas != canvas)
            {
                continue;
            }

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                continue;
            }

            if (canvas.GetComponent<AdaptiveUIRuntime>() == null)
            {
                canvas.gameObject.AddComponent<AdaptiveUIRuntime>();
            }
        }
    }

    private static void ConfigureScaler(CanvasScaler scaler)
    {
        if (scaler == null)
        {
            return;
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        float screenAspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
        float referenceAspect = scaler.referenceResolution.x / scaler.referenceResolution.y;
        scaler.matchWidthOrHeight = screenAspect >= referenceAspect ? 1f : 0f;
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void OnEnable()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        ApplySafeArea();
    }

    private void Update()
    {
        Vector2Int currentScreen = new Vector2Int(Screen.width, Screen.height);
        Rect currentSafeArea = Screen.safeArea;

        if (currentScreen != _lastScreenSize || currentSafeArea != _lastSafeArea)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        if (_rectTransform == null)
        {
            return;
        }

        float screenWidth = Mathf.Max(1f, Screen.width);
        float screenHeight = Mathf.Max(1f, Screen.height);
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= screenWidth;
        anchorMin.y /= screenHeight;
        anchorMax.x /= screenWidth;
        anchorMax.y /= screenHeight;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;

        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastSafeArea = safeArea;
    }
}
