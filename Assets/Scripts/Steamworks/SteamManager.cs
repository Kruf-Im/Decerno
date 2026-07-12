using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class SteamManager : MonoBehaviour
{
    private const uint AppId = 480;

    public static bool IsInitialized => _isInitialized;

    private static SteamManager _instance;
    private static bool _isInitialized;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSteam();
    }

    private void Update()
    {
        if (_isInitialized)
            SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        ShutdownSteam();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            ShutdownSteam();
        }
    }

    private void InitializeSteam()
    {
        if (_isInitialized) return;

        try
        {
            SteamClient.Init(AppId, true);
            _isInitialized = true;
            Debug.Log($"[SteamManager] Initialized Steam! User: {SteamClient.Name} ({SteamClient.SteamId})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SteamManager] Could not initialize Steam: {e.Message}");
        }
    }

    private static void ShutdownSteam()
    {
        if (!_isInitialized) return;

        try
        {
            SteamClient.Shutdown();
            _isInitialized = false;
            Debug.Log("[SteamManager] SteamClient Shutdown Successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SteamManager] Error shutting down Steam: {e.Message}");
        }
    }
}