using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public class BackgroundController : MonoBehaviour {
  [Header("Background Assets")]
  [SerializeField] string resourceFolder = "Art/Backgrounds";
  [SerializeField] Vector2Int generatedResolution = new Vector2Int(1920, 1080);
  [SerializeField] Color topColor = new Color(0.025f, 0.047f, 0.094f);
  [SerializeField] Color bottomColor = new Color(0.003f, 0.012f, 0.027f);
  [SerializeField, Range(0f, 0.02f)] float starDensity = 0.0025f;
  [SerializeField] Vector2 starBrightness = new Vector2(0.6f, 1.1f);

  Canvas _canvas;
  Image _image;
  Sprite _generatedSprite;
  Texture2D _generatedTexture;
  Sprite _loadedSprite;

  void Awake() {
    CreateCanvas();
    ApplyBackground();
  }

  void CreateCanvas() {
    if (_canvas != null) return;
    var canvasGO = new GameObject("BackdropCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    canvasGO.transform.SetParent(transform, false);
    _canvas = canvasGO.GetComponent<Canvas>();
    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    _canvas.sortingOrder = -100;
    var scaler = canvasGO.GetComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920f, 1080f);
    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    scaler.matchWidthOrHeight = 1f;

    var imageGO = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    imageGO.transform.SetParent(canvasGO.transform, false);
    _image = imageGO.GetComponent<Image>();
    var rect = _image.rectTransform;
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
    _image.raycastTarget = false;
  }

  void ApplyBackground() {
    _loadedSprite = ChooseResourceSprite();
    if (_loadedSprite == null) _loadedSprite = BuildProceduralSprite();

    if (_image != null && _loadedSprite != null) {
      _image.sprite = _loadedSprite;
      _image.color = Color.white;
      _image.type = _loadedSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
      _image.preserveAspect = false;
    }
  }

  Sprite ChooseResourceSprite() {
    if (string.IsNullOrEmpty(resourceFolder)) return null;
    var sprites = UIResourceHelper.LoadSpritesInFolder(resourceFolder);
    if (sprites.Length == 0) {
      return UIResourceHelper.LoadSprite(resourceFolder);
    }
    int index = sprites.Length == 1 ? 0 : UnityEngine.Random.Range(0, sprites.Length);
    return sprites.ElementAtOrDefault(index);
  }

  Sprite BuildProceduralSprite() {
    var tex = GenerateStarfieldTexture(Mathf.Max(4, generatedResolution.x), Mathf.Max(4, generatedResolution.y));
    if (tex == null) return null;
    tex.wrapMode = TextureWrapMode.Clamp;
    _generatedTexture = tex;
    _generatedSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    return _generatedSprite;
  }

  Texture2D GenerateStarfieldTexture(int width, int height) {
    var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    var pixels = new Color32[width * height];

    for (int y = 0; y < height; y++) {
      float t = height > 1 ? (float)y / (height - 1) : 0f;
      Color baseColor = Color.Lerp(bottomColor, topColor, t);
      var baseColor32 = (Color32)baseColor;
      for (int x = 0; x < width; x++) {
        pixels[y * width + x] = baseColor32;
      }
    }

    int starCount = Mathf.CeilToInt(width * height * Mathf.Clamp(starDensity, 0f, 0.05f));
    var rng = new System.Random(Environment.TickCount);
    for (int i = 0; i < starCount; i++) {
      int px = rng.Next(width);
      int py = rng.Next(height);
      float intensity = Mathf.Lerp(starBrightness.x, starBrightness.y, (float)rng.NextDouble());
      DrawStar(pixels, width, height, px, py, intensity);
    }

    texture.SetPixels32(pixels);
    texture.Apply();
    return texture;
  }

  void DrawStar(Color32[] pixels, int width, int height, int cx, int cy, float intensity) {
    int radius = Mathf.Clamp(Mathf.RoundToInt(intensity * 2f), 1, 3);
    for (int y = -radius; y <= radius; y++) {
      int yy = cy + y;
      if (yy < 0 || yy >= height) continue;
      for (int x = -radius; x <= radius; x++) {
        int xx = cx + x;
        if (xx < 0 || xx >= width) continue;
        float dist = Mathf.Sqrt(x * x + y * y);
        if (dist > radius) continue;
        float falloff = Mathf.Clamp01(1f - dist / (radius + 0.25f));
        float brightness = Mathf.Clamp01(intensity * falloff);
        int index = yy * width + xx;
        var existing = pixels[index];
        byte add = (byte)Mathf.Clamp(brightness * 255f, 0f, 255f);
        pixels[index] = new Color32(
          (byte)Mathf.Clamp(existing.r + add, 0, 255),
          (byte)Mathf.Clamp(existing.g + add, 0, 255),
          (byte)Mathf.Clamp(existing.b + add, 0, 255),
          255);
      }
    }
  }

  void OnDestroy() {
    if (_generatedSprite != null) Destroy(_generatedSprite);
    if (_generatedTexture != null) Destroy(_generatedTexture);
  }
}
