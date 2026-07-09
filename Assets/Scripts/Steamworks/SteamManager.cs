using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class SteamManager : MonoBehaviour
{
    // Default test AppID (Spacewar). Replace with your actual AppID later.
    private const uint AppId = 480;

    private static SteamManager instance;

    private void Awake()
    {
        // Ensure only one instance of SteamManager exists
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            // Initialize Steamworks client
            SteamClient.Init(AppId, true);
            Debug.Log($"[SteamManager] Successfully initialized Steam! Logged in as: {SteamClient.Name} ({SteamClient.SteamId})");
        }
        catch (System.Exception e)
        {
            // Steam isn't running, or steam_api64.dll is missing
            Debug.LogError($"[SteamManager] Could not initialize Steam: {e.Message}");
        }
    }

    private void Update()
    {
        // Run callbacks every frame to process Steam events
        SteamClient.RunCallbacks();
    }

    private void OnDisable()
    {
        // Cleanly shutdown when leaving play mode or quitting
        SteamClient.Shutdown();
    }
}