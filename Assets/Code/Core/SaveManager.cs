using System;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class SaveManager : MonoBehaviour {
  private const string SlotName = "main";
  void Awake(){ ServiceLocator.Register(this); }
  void Start(){
    LoadGame();
  }
  void OnApplicationPause(bool pause){ if (pause) SaveGame(); }
  void OnApplicationQuit(){ SaveGame(); }
  public void SaveGame(){
    var snapshot = new GameStateSnapshot { savedAtUtcTicks = DateTime.UtcNow.Ticks };
    if (ServiceLocator.TryGet<EconomyService>(out var econ)) snapshot.economy = econ.ExportSnapshot();
    if (ServiceLocator.TryGet<ScanService>(out var scan)) snapshot.scan = scan.ExportSnapshot();
    if (ServiceLocator.TryGet<MissionService>(out var missions)) snapshot.missions = missions.ExportSnapshot();
    var json = JsonUtil.ToJson(snapshot);
    SaveSystem.Save(SlotName, json);
  }
  public void LoadGame(){
    var json = SaveSystem.Load(SlotName);
    if (string.IsNullOrEmpty(json)) return;
    var snapshot = JsonUtil.FromJson<GameStateSnapshot>(json);
    if (snapshot == null) return;
    if (ServiceLocator.TryGet<EconomyService>(out var econ) && snapshot.economy != null) econ.ImportSnapshot(snapshot.economy);
    if (ServiceLocator.TryGet<ScanService>(out var scan) && snapshot.scan != null) scan.ImportSnapshot(snapshot.scan);
    if (ServiceLocator.TryGet<MissionService>(out var missions)) {
      missions.ImportSnapshot(snapshot.missions ?? Array.Empty<MissionSnapshot>());
      if (snapshot.savedAtUtcTicks > 0) {
        var savedAt = new DateTime(snapshot.savedAtUtcTicks, DateTimeKind.Utc);
        var elapsed = (DateTime.UtcNow - savedAt).TotalSeconds;
        if (elapsed > 0) missions.SimulateOffline(elapsed);
      }
    }
  }
}
