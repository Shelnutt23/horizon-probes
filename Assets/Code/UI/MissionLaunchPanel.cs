using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionLaunchPanel : MonoBehaviour {
  public Rect panelRect = new Rect(450f, 170f, 420f, 520f);
  public int scanEnergyCost = 1;
  public LoadoutSelectionPanel loadoutPanel;

  MissionJson[] _missions = Array.Empty<MissionJson>();
  int _selectedMissionIndex = -1;
  Vector2 _missionScroll;
  string _statusMessage;
  LoadoutBuilder _builder;
  MissionService _missionService;
  ScanService _scanService;

  void Start() {
    if (loadoutPanel == null) loadoutPanel = GetComponent<LoadoutSelectionPanel>();
    _builder = ServiceLocator.Get<LoadoutBuilder>();
    _missionService = ServiceLocator.Get<MissionService>();
    ServiceLocator.TryGet(out _scanService);
    var missionList = ContentLoader.Missions;
    if (missionList?.missions != null) _missions = missionList.missions;
  }

  void OnGUI() {
    GUILayout.BeginArea(panelRect, GUI.skin.box);
    GUILayout.Label("Mission Launch", GUI.skin.label);
    DrawMissionSelector();
    DrawMissionDetails();
    DrawLaunchControls();
    DrawStatus();
    GUILayout.EndArea();
  }

  void DrawMissionSelector() {
    GUILayout.Label("Available Missions", GUI.skin.boldLabel);
    if (_missions.Length == 0) {
      GUILayout.Label("No missions defined.");
      return;
    }
    _missionScroll = GUILayout.BeginScrollView(_missionScroll, GUILayout.Height(160));
    for (int i = 0; i < _missions.Length; i++) {
      bool selected = i == _selectedMissionIndex;
      var mission = _missions[i];
      var prev = GUI.color;
      if (selected) GUI.color = Color.cyan;
      if (GUILayout.Button($"{mission.id} ({mission.type})")) {
        _selectedMissionIndex = i;
        NotifyMissionTypeChanged();
      }
      GUI.color = prev;
    }
    GUILayout.EndScrollView();
  }

  void DrawMissionDetails() {
    if (_selectedMissionIndex < 0 || _selectedMissionIndex >= _missions.Length) {
      GUILayout.Label("Select a mission to view details.");
      return;
    }
    var mission = _missions[_selectedMissionIndex];
    GUILayout.Label($"Duration: {mission.baseDurationS} s");
    GUILayout.Label($"Tick: {mission.tickS} s");
    GUILayout.Label($"Base Yield - M:{mission.baseM} V:{mission.baseV} D:{mission.baseD} X:{mission.baseX:F1}");
    if (loadoutPanel?.CurrentStats != null) {
      var stats = loadoutPanel.CurrentStats;
      GUILayout.Label("-- Loadout Preview --", GUI.skin.boldLabel);
      GUILayout.Label($"Autonomy {stats.Autonomy:F2}  Reliability {stats.Reliability:F2}");
      GUILayout.Label($"Throughput {stats.Throughput:F2}  Stealth {stats.Stealth:F2}");
    }
  }

  void DrawLaunchControls() {
    GUILayout.Space(6f);
    List<string> issues = new List<string>();
    if (_builder == null) issues.Add("Loadout builder unavailable.");
    if (_missionService == null) issues.Add("Mission service unavailable.");
    if (_selectedMissionIndex < 0) issues.Add("Select a mission.");
    var config = loadoutPanel?.CurrentConfiguration;
    if (_builder != null && config != null) issues.AddRange(_builder.ValidateConfiguration(config));
    if (_scanService != null && _scanService.Current < scanEnergyCost)
      issues.Add($"Requires {scanEnergyCost} scan energy (available {_scanService.Current}).");

    bool canLaunch = issues.Count == 0;
    GUI.enabled = canLaunch;
    if (GUILayout.Button("Launch Mission")) LaunchSelectedMission();
    GUI.enabled = true;

    if (issues.Count > 0) {
      GUILayout.Space(4f);
      GUILayout.Label("Launch requirements:");
      foreach (var issue in issues) GUILayout.Label("• " + issue);
    }
  }

  void DrawStatus() {
    if (string.IsNullOrEmpty(_statusMessage)) return;
    GUILayout.Space(6f);
    GUILayout.Label(_statusMessage);
  }

  void LaunchSelectedMission() {
    if (_selectedMissionIndex < 0 || _selectedMissionIndex >= _missions.Length) {
      _statusMessage = "Select a mission before launching.";
      return;
    }
    if (_builder == null || loadoutPanel == null) {
      _statusMessage = "Missing required services.";
      return;
    }
    var mission = _missions[_selectedMissionIndex];
    if (!_builder.TryCreateMissionInstance(mission, loadoutPanel.CurrentConfiguration, out var instance, out var error)) {
      _statusMessage = error;
      return;
    }
    if (_scanService != null && !_scanService.Consume(scanEnergyCost)) {
      _statusMessage = "Insufficient scan energy.";
      return;
    }
    if (_missionService == null) {
      _statusMessage = "Mission service unavailable.";
      return;
    }
    instance.startUtc = DateTime.UtcNow;
    _missionService.Enqueue(instance);
    _statusMessage = $"Mission {mission.id} launched.";
  }

  void NotifyMissionTypeChanged() {
    if (loadoutPanel == null || _selectedMissionIndex < 0 || _selectedMissionIndex >= _missions.Length) return;
    var missionType = LoadoutBuilder.ParseMissionType(_missions[_selectedMissionIndex].type);
    loadoutPanel.SetMissionType(missionType);
  }
}
