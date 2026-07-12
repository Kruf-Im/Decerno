using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SteamUtils
{
    public static async Task<Texture2D> LoadProfilePicture(SteamId steamId)
    {
        Steamworks.Data.Image? avatar = await SteamFriends.GetLargeAvatarAsync(steamId);
        if (!avatar.HasValue)
            return null;
        Steamworks.Data.Image image = avatar.Value;

        Texture2D texture = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        for(int x = 0; x< image.Width; x++)
        {
            for(int y = 0; y< image.Height; y++)
            {
                var pixel = image.GetPixel(x, y);
                texture.SetPixel(x, (int)image.Height - 1 - y, new Color32(pixel.r, pixel.g, pixel.b, pixel.a));
            }
        }

        texture.Apply();
        return texture;
    }
}
