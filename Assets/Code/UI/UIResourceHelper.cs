using UnityEngine;

public static class UIResourceHelper {
  public static Sprite LoadSprite(string path) {
    if (string.IsNullOrEmpty(path)) return null;
    var sprite = Resources.Load<Sprite>(path);
    if (sprite != null) return sprite;
    var texture = Resources.Load<Texture2D>(path);
    if (texture == null) return null;
    return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
  }

  public static Texture2D LoadTexture(string path) {
    if (string.IsNullOrEmpty(path)) return null;
    var texture = Resources.Load<Texture2D>(path);
    if (texture != null) return texture;
    var sprite = Resources.Load<Sprite>(path);
    return sprite != null ? sprite.texture : null;
  }

  public static Sprite[] LoadSpritesInFolder(string folderPath) {
    if (string.IsNullOrEmpty(folderPath)) return System.Array.Empty<Sprite>();
    var sprites = Resources.LoadAll<Sprite>(folderPath);
    return sprites ?? System.Array.Empty<Sprite>();
  }
}
