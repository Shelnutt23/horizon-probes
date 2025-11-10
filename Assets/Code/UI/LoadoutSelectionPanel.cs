using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadoutSelectionPanel : MonoBehaviour {
  public Rect panelRect = new Rect(10f, 170f, 420f, 520f);
  LoadoutBuilder _builder;
  ChassisJson[] _chassis = Array.Empty<ChassisJson>();
  IReadOnlyList<ModuleJson> _payloadModules = Array.Empty<ModuleJson>();
  IReadOnlyList<ModuleJson> _powerModules = Array.Empty<ModuleJson>();
  IReadOnlyList<ModuleJson> _utilityModules = Array.Empty<ModuleJson>();

  int _selectedChassisIndex = -1;
  int _selectedPayloadIndex;
  int _selectedPowerIndex;
  readonly List<int> _selectedUtilityIndices = new List<int>();
  Vector2 _chassisScroll;
  MissionType? _missionType;

  public LoadoutBuilder.LoadoutConfiguration CurrentConfiguration { get; private set; } = new LoadoutBuilder.LoadoutConfiguration();
  public LoadoutBuilder.LoadoutStats CurrentStats { get; private set; }
  void Start() {
    _builder = ServiceLocator.Get<LoadoutBuilder>();
    if (_builder == null) {
      Debug.LogError("LoadoutSelectionPanel requires LoadoutBuilder service in the scene.");
      return;
    }
    _chassis = _builder.AvailableChassis.ToArray();
    _payloadModules = _builder.GetModulesForSlot("payload");
    _powerModules = _builder.GetModulesForSlot("power");
    _utilityModules = _builder.GetModulesForSlot("utility");
    _builder.EnsureUtilitySlotSize(CurrentConfiguration);
  }

  public void SetMissionType(MissionType? missionType) {
    _missionType = missionType;
    RecalculateStats();
  }

  void OnGUI() {
    if (_builder == null) {
      GUILayout.BeginArea(panelRect, GUI.skin.box);
      GUILayout.Label("Loadout builder unavailable.");
      GUILayout.EndArea();
      return;
    }
    GUILayout.BeginArea(panelRect, GUI.skin.box);
    GUILayout.Label("Loadout Configuration", GUI.skin.label);
    DrawChassisSelector();
    DrawModuleSelectors();
    DrawStats();
    GUILayout.EndArea();
  }

  void DrawChassisSelector() {
    GUILayout.Label("Chassis", GUI.skin.boldLabel);
    if (_chassis.Length == 0) {
      GUILayout.Label("No chassis data found.");
      return;
    }
    _chassisScroll = GUILayout.BeginScrollView(_chassisScroll, GUILayout.Height(140));
    for (int i = 0; i < _chassis.Length; i++) {
      var chassis = _chassis[i];
      bool isSelected = i == _selectedChassisIndex;
      var prevColor = GUI.color;
      if (isSelected) GUI.color = Color.cyan;
      if (GUILayout.Button($"{chassis.id}  (slots: {chassis.baseSlots})")) {
        if (_selectedChassisIndex != i) {
          _selectedChassisIndex = i;
          CurrentConfiguration.Chassis = chassis;
          _builder.EnsureUtilitySlotSize(CurrentConfiguration);
          SyncUtilitySelectionCount();
          RecalculateStats();
        }
      }
      GUI.color = prevColor;
    }
    GUILayout.EndScrollView();
  }

  void DrawModuleSelectors() {
    GUILayout.Space(8f);
    GUILayout.Label("Payload Modules", GUI.skin.boldLabel);
    _selectedPayloadIndex = DrawModuleSelectionGrid(_payloadModules, _selectedPayloadIndex, out var payloadSelection);
    CurrentConfiguration.PayloadModule = payloadSelection;

    GUILayout.Space(4f);
    GUILayout.Label("Power Modules", GUI.skin.boldLabel);
    _selectedPowerIndex = DrawModuleSelectionGrid(_powerModules, _selectedPowerIndex, out var powerSelection);
    CurrentConfiguration.PowerModule = powerSelection;

    GUILayout.Space(4f);
    GUILayout.Label("Utility Modules", GUI.skin.boldLabel);
    if (CurrentConfiguration.Chassis == null) {
      GUILayout.Label("Select a chassis to configure utility slots.");
    } else {
      SyncUtilitySelectionCount();
      for (int i = 0; i < _selectedUtilityIndices.Count; i++) {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Slot {i + 1}", GUILayout.Width(60f));
        _selectedUtilityIndices[i] = DrawModuleSelectionGrid(_utilityModules, _selectedUtilityIndices[i], out var utilityModule, true);
        if (i < CurrentConfiguration.UtilityModules.Count) CurrentConfiguration.UtilityModules[i] = utilityModule;
        GUILayout.EndHorizontal();
      }
    }
    RecalculateStats();
  }

  int DrawModuleSelectionGrid(IReadOnlyList<ModuleJson> modules, int currentIndex, out ModuleJson selectedModule, bool compact = false) {
    var names = new string[Math.Max(1, (modules?.Count ?? 0) + 1)];
    names[0] = "None";
    if (modules != null) {
      for (int i = 0; i < modules.Count; i++) names[i + 1] = modules[i].id;
    }
    if (currentIndex < 0 || currentIndex >= names.Length) currentIndex = 0;
    int gridWidth = compact ? 1 : 1;
    currentIndex = GUILayout.SelectionGrid(currentIndex, names, gridWidth);
    selectedModule = null;
    if (modules != null && currentIndex > 0 && currentIndex - 1 < modules.Count) selectedModule = modules[currentIndex - 1];
    return currentIndex;
  }

  void SyncUtilitySelectionCount() {
    int desired = _builder.GetUtilitySlotCount(CurrentConfiguration.Chassis);
    while (_selectedUtilityIndices.Count < desired) _selectedUtilityIndices.Add(0);
    while (_selectedUtilityIndices.Count > desired) _selectedUtilityIndices.RemoveAt(_selectedUtilityIndices.Count - 1);
    _builder.EnsureUtilitySlotSize(CurrentConfiguration);
  }

  void RecalculateStats() {
    CurrentStats = null;
    if (CurrentConfiguration.Chassis == null) return;
    var stats = _builder.CalculateStats(CurrentConfiguration);
    if (stats != null && ServiceLocator.TryGet<TechService>(out var tech)) {
      var missionType = _missionType ?? MissionType.Survey;
      tech.ApplyTechToLoadout(stats, missionType);
    }
    CurrentStats = stats;
  }

  void DrawStats() {
    GUILayout.Space(8f);
    GUILayout.Label("Loadout Stats", GUI.skin.boldLabel);
    if (CurrentStats == null) {
      GUILayout.Label("Select a chassis and modules to preview stats.");
      return;
    }
    GUILayout.Label($"Autonomy: {CurrentStats.Autonomy:F2}");
    GUILayout.Label($"Reliability: {CurrentStats.Reliability:F2}");
    GUILayout.Label($"Throughput: {CurrentStats.Throughput:F2}");
    GUILayout.Label($"Stealth: {CurrentStats.Stealth:F2}");
    GUILayout.Label($"Generic Mitigation: {_builder.EvaluateGenericMitigation(CurrentStats):P0}");
    GUILayout.Label($"Yield Multipliers - M:{CurrentStats.GetResourceMultiplier("M"):F2} V:{CurrentStats.GetResourceMultiplier("V"):F2} D:{CurrentStats.GetResourceMultiplier("D"):F2} X:{CurrentStats.GetResourceMultiplier("X"):F2}");
  }
}
