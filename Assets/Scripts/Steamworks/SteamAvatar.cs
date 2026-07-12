using UnityEngine;
using UnityEngine.UI;

public class SteamAvatar : MonoBehaviour
{
    [SerializeField] private Image DisplayImage;
    private async void Start()
    {
        if (DisplayImage == null)
        {
            Debug.LogError("[SteamAvatar] DisplayImage is not assigned!");
            return;
        }
        if (!SteamManager.IsInitialized)
        {
            Debug.LogError("[SteamAvatar] SteamManager is not initialized!");
            return;
        }
        var steamId = Steamworks.SteamClient.SteamId;
        var avatar = await SteamUtils.LoadProfilePicture(steamId);
        if (avatar != null)
        {
            Rect rect = new Rect(0, 0, avatar.width, avatar.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite avatarSprite = Sprite.Create(avatar, rect, pivot);
            avatarSprite.name = Steamworks.SteamClient.Name + "Avatar";
            DisplayImage.sprite = avatarSprite;
            Debug.Log($"[SteamAvatar] Loaded avatar for user: {Steamworks.SteamClient.Name}");
        }
        else
        {
            Debug.LogWarning("[SteamAvatar] Could not load avatar.");
        }
    }
}
