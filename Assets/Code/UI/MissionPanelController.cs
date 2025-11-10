using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MissionPanelController : MonoBehaviour {
  private class MissionEntryView {
    public MissionInstance Instance;
    public GameObject Root;
    public Text Title;
    public Text Status;
  }

  [Header("Theme Assets")]
  [SerializeField] string panelBackgroundSpritePath = "Art/UI/mission_panel_bg";
  [SerializeField] string sectionBackgroundSpritePath = "Art/UI/mission_section_bg";
  [SerializeField] string buttonSpritePath = "Art/UI/mission_button_bg";

  private readonly Dictionary<MissionInstance, MissionEntryView> _activeEntries = new Dictionary<MissionInstance, MissionEntryView>();

  private RectTransform _missionButtonRoot;
  private Button _missionButtonTemplate;
  private RectTransform _activeMissionRoot;
  private GameObject _activeMissionTemplate;
  private Text _economyStatus;

  private MissionService _missionService;
  private EconomyService _economyService;
  private Sprite _panelBackgroundSprite;
  private Sprite _sectionBackgroundSprite;
  private Sprite _buttonBackgroundSprite;

  void Awake() {
    LoadThemeSprites();
    EnsureEventSystem();
    BuildUI();
  }

  void Start() {
    ServiceLocator.TryGet(out _missionService);
    ServiceLocator.TryGet(out _economyService);
    PopulateMissionButtons();
  }

  void Update() {
    UpdateEconomyReadout();
    UpdateActiveMissions();
  }

  private void PopulateMissionButtons() {
    if (_missionButtonRoot == null || _missionButtonTemplate == null) return;
    foreach (Transform child in _missionButtonRoot) {
      if (child.gameObject != _missionButtonTemplate.gameObject) Destroy(child.gameObject);
    }
    _missionButtonTemplate.gameObject.SetActive(false);

    var missionContent = ContentLoader.Missions;
    if (missionContent?.missions == null) return;

    foreach (var mission in missionContent.missions) {
      var button = Instantiate(_missionButtonTemplate, _missionButtonRoot);
      button.gameObject.SetActive(true);
      if (button.TryGetComponent(out LayoutElement layout)) layout.flexibleHeight = 0;
      var label = button.GetComponentInChildren<Text>();
      if (label != null) label.text = FormatMissionLabel(mission);
      button.onClick.AddListener(() => TryQueueMission(mission));
    }
  }

  private string FormatMissionLabel(MissionJson mission) {
    var duration = TimeSpan.FromSeconds(Mathf.Max(1, mission.baseDurationS));
    string durationText = duration.TotalHours >= 1
      ? $"{Mathf.CeilToInt((float)duration.TotalHours)}h"
      : duration.TotalMinutes >= 1 ? $"{Mathf.CeilToInt((float)duration.TotalMinutes)}m" : $"{Mathf.CeilToInt((float)duration.TotalSeconds)}s";
    return $"{mission.id}\nCost: M{mission.baseM} V{mission.baseV} D{mission.baseD} • {durationText}";
  }

  private void TryQueueMission(MissionJson mission) {
    if (_missionService == null) return;
    if (_economyService != null && !_economyService.Spend(mission.baseM, mission.baseV, mission.baseD)) {
      return;
    }

    var instance = BuildMissionInstance(mission);
    _missionService.Enqueue(instance);
  }

  private MissionInstance BuildMissionInstance(MissionJson mission) {
    var instance = new MissionInstance {
      missionId = mission.id,
      type = ParseMissionType(mission.type),
      tickS = Mathf.Max(1, mission.tickS),
      ticksPlanned = Mathf.Max(1, Mathf.CeilToInt(mission.baseDurationS / Mathf.Max(1f, mission.tickS))),
      ticksRun = 0,
      startUtc = DateTime.UtcNow,
      baseFail = mission.baseFail,
      baseTickYield = default,
      yieldsSoFar = default,
      missionSeed = CreateMissionSeed(mission.id)
    };
    return instance;
  }

  private ulong CreateMissionSeed(string missionId) {
    ulong hash = 1469598103934665603UL;
    foreach (char c in missionId) {
      hash ^= c;
      hash *= 1099511628211UL;
    }
    return RNG.Seed(hash, (ulong)DateTime.UtcNow.Ticks);
  }

  private MissionType ParseMissionType(string type) {
    if (Enum.TryParse(type, true, out MissionType parsed)) return parsed;
    return MissionType.Survey;
  }

  private void UpdateActiveMissions() {
    if (_missionService == null || _activeMissionRoot == null || _activeMissionTemplate == null) return;

    var active = _missionService.Active;
    var existingKeys = _activeEntries.Keys.ToList();
    foreach (var key in existingKeys) {
      if (!active.Contains(key)) {
        if (_activeEntries[key].Root != null) Destroy(_activeEntries[key].Root);
        _activeEntries.Remove(key);
      }
    }

    foreach (var mission in active) {
      if (!_activeEntries.ContainsKey(mission)) {
        _activeEntries[mission] = CreateActiveEntry(mission);
      }
      UpdateActiveEntry(_activeEntries[mission]);
    }
  }

  private MissionEntryView CreateActiveEntry(MissionInstance mission) {
    var entryGO = Instantiate(_activeMissionTemplate, _activeMissionRoot);
    entryGO.SetActive(true);
    var texts = entryGO.GetComponentsInChildren<Text>();
    Text title = texts.Length > 0 ? texts[0] : null;
    Text status = texts.Length > 1 ? texts[1] : null;
    if (title != null) title.text = mission.missionId;
    return new MissionEntryView { Instance = mission, Root = entryGO, Title = title, Status = status };
  }

  private void UpdateActiveEntry(MissionEntryView entry) {
    if (entry.Instance == null || entry.Status == null) return;
    var mission = entry.Instance;
    int ticksRemaining = Mathf.Max(0, mission.ticksPlanned - mission.ticksRun);
    int secondsRemaining = ticksRemaining * Mathf.Max(1, mission.tickS);
    var remaining = TimeSpan.FromSeconds(secondsRemaining);
    string remainingText = remaining.TotalHours >= 1
      ? $"{remaining.TotalHours:F1}h"
      : remaining.TotalMinutes >= 1 ? $"{remaining.TotalMinutes:F1}m" : $"{remaining.TotalSeconds:F0}s";
    entry.Status.text = $"Progress: {mission.ticksRun}/{mission.ticksPlanned} — {remainingText} remaining";
  }

  private void UpdateEconomyReadout() {
    if (_economyStatus == null || _economyService == null) return;
    _economyStatus.text = $"Resources — M:{_economyService.M} V:{_economyService.V} D:{_economyService.D}";
  }

  private void EnsureEventSystem() {
    if (FindObjectOfType<EventSystem>() != null) return;
    var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    DontDestroyOnLoad(eventSystem);
  }

  private void BuildUI() {
    var canvasGO = new GameObject("MissionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    var canvas = canvasGO.GetComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 100;
    var scaler = canvasGO.GetComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920, 1080);
    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    scaler.matchWidthOrHeight = 1f;
    canvasGO.transform.SetParent(null);

    var panelGO = new GameObject("MissionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
    panelGO.transform.SetParent(canvasGO.transform, false);
    var panelRect = panelGO.GetComponent<RectTransform>();
    panelRect.anchorMin = new Vector2(1f, 1f);
    panelRect.anchorMax = new Vector2(1f, 1f);
    panelRect.pivot = new Vector2(1f, 1f);
    panelRect.anchoredPosition = new Vector2(-24f, -24f);
    panelRect.sizeDelta = new Vector2(360f, 520f);
    var panelImage = panelGO.GetComponent<Image>();
    if (_panelBackgroundSprite != null) {
      panelImage.sprite = _panelBackgroundSprite;
      panelImage.color = Color.white;
      panelImage.type = _panelBackgroundSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
    } else {
      panelImage.color = new Color(0.12f, 0.16f, 0.2f, 0.9f);
    }
    var panelLayout = panelGO.GetComponent<VerticalLayoutGroup>();
    panelLayout.padding = new RectOffset(12, 12, 12, 12);
    panelLayout.spacing = 12f;
    panelLayout.childForceExpandHeight = false;

    var header = CreateText("Mission Control", panelGO.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
    var econText = CreateText("Resources — M:0 V:0 D:0", panelGO.transform, 16, FontStyle.Normal, TextAnchor.MiddleCenter);
    _economyStatus = econText;

    var availableLabel = CreateText("Available Missions", panelGO.transform, 18, FontStyle.Bold, TextAnchor.MiddleLeft);
    var missionSection = CreateSection(panelGO.transform, true, out _missionButtonRoot);
    _missionButtonTemplate = CreateButton("MissionButtonTemplate", missionSection.transform);
    _missionButtonTemplate.gameObject.SetActive(false);

    var activeLabel = CreateText("Active Missions", panelGO.transform, 18, FontStyle.Bold, TextAnchor.MiddleLeft);
    var activeSection = CreateSection(panelGO.transform, false, out _activeMissionRoot);
    _activeMissionTemplate = CreateActiveMissionTemplate(activeSection.transform);
    _activeMissionTemplate.SetActive(false);
  }

  private RectTransform CreateSection(Transform parent, bool flexibleHeight, out RectTransform contentRoot) {
    var sectionGO = new GameObject("Section", typeof(RectTransform), typeof(LayoutElement));
    sectionGO.transform.SetParent(parent, false);
    var layout = sectionGO.GetComponent<LayoutElement>();
    layout.flexibleHeight = flexibleHeight ? 1f : 0f;
    layout.preferredHeight = flexibleHeight ? -1f : 140f;
    var sectionRect = sectionGO.GetComponent<RectTransform>();
    sectionRect.anchorMin = new Vector2(0, 0);
    sectionRect.anchorMax = new Vector2(1, 1);
    sectionRect.offsetMin = Vector2.zero;
    sectionRect.offsetMax = Vector2.zero;

    var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    background.transform.SetParent(sectionGO.transform, false);
    var bgRect = background.GetComponent<RectTransform>();
    bgRect.anchorMin = Vector2.zero;
    bgRect.anchorMax = Vector2.one;
    bgRect.offsetMin = new Vector2(0, 0);
    bgRect.offsetMax = new Vector2(0, 0);
    var backgroundImage = background.GetComponent<Image>();
    if (_sectionBackgroundSprite != null) {
      backgroundImage.sprite = _sectionBackgroundSprite;
      backgroundImage.color = Color.white;
      backgroundImage.type = _sectionBackgroundSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
    } else {
      backgroundImage.color = new Color(0f, 0f, 0f, 0.3f);
    }

    var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
    content.transform.SetParent(background.transform, false);
    var contentRect = content.GetComponent<RectTransform>();
    contentRect.anchorMin = new Vector2(0, 0);
    contentRect.anchorMax = new Vector2(1, 1);
    contentRect.offsetMin = new Vector2(8, 8);
    contentRect.offsetMax = new Vector2(-8, -8);
    var contentLayout = content.GetComponent<VerticalLayoutGroup>();
    contentLayout.spacing = 8f;
    contentLayout.childForceExpandHeight = false;
    contentLayout.childControlHeight = true;
    contentLayout.childControlWidth = true;
    contentLayout.childForceExpandWidth = true;

    contentRoot = contentRect;
    return sectionRect;
  }

  private Button CreateButton(string name, Transform parent) {
    var buttonGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
    buttonGO.transform.SetParent(parent, false);
    var image = buttonGO.GetComponent<Image>();
    if (_buttonBackgroundSprite != null) {
      image.sprite = _buttonBackgroundSprite;
      image.color = Color.white;
      image.type = _buttonBackgroundSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
    } else {
      image.color = new Color(0.2f, 0.35f, 0.45f, 0.9f);
    }
    var text = CreateText("Button", buttonGO.transform, 16, FontStyle.Normal, TextAnchor.MiddleCenter);
    text.rectTransform.anchorMin = Vector2.zero;
    text.rectTransform.anchorMax = Vector2.one;
    text.rectTransform.offsetMin = new Vector2(12, 8);
    text.rectTransform.offsetMax = new Vector2(-12, -8);
    return buttonGO.GetComponent<Button>();
  }

  private GameObject CreateActiveMissionTemplate(Transform parent) {
    var entryGO = new GameObject("ActiveMissionTemplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    entryGO.transform.SetParent(parent, false);
    var image = entryGO.GetComponent<Image>();
    if (_sectionBackgroundSprite != null) {
      image.sprite = _sectionBackgroundSprite;
      image.color = Color.white;
      image.type = _sectionBackgroundSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
    } else {
      image.color = new Color(0.15f, 0.22f, 0.28f, 0.85f);
    }

    var layout = entryGO.AddComponent<VerticalLayoutGroup>();
    layout.spacing = 2f;
    layout.padding = new RectOffset(8, 8, 6, 6);
    layout.childControlHeight = true;
    layout.childControlWidth = true;

    CreateText("Mission Name", entryGO.transform, 16, FontStyle.Bold, TextAnchor.MiddleLeft);
    CreateText("Status", entryGO.transform, 14, FontStyle.Normal, TextAnchor.MiddleLeft);

    return entryGO;
  }

  private Text CreateText(string text, Transform parent, int size, FontStyle style, TextAnchor anchor) {
    var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
    go.transform.SetParent(parent, false);
    var uiText = go.GetComponent<Text>();
    uiText.text = text;
    uiText.fontSize = size;
    uiText.fontStyle = style;
    uiText.alignment = anchor;
    uiText.color = new Color(0.92f, 0.95f, 1f, 1f);
    uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    return uiText;
  }

  void LoadThemeSprites() {
    _panelBackgroundSprite = UIResourceHelper.LoadSprite(panelBackgroundSpritePath);
    _sectionBackgroundSprite = UIResourceHelper.LoadSprite(sectionBackgroundSpritePath);
    _buttonBackgroundSprite = UIResourceHelper.LoadSprite(buttonSpritePath);
  }
}
