using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionService : MonoBehaviour {
  public readonly List<MissionInstance> Active = new List<MissionInstance>();
  private float _accum;

  void Awake(){ ServiceLocator.Register(this); }

  public void Enqueue(MissionInstance m) {
    if (ServiceLocator.TryGet<TechService>(out var tech)) tech.ApplyTechToMission(m);
    Active.Add(m);
  }

  void Update() {
    _accum += Time.deltaTime;
    while (_accum >= 1f) { _accum -= 1f; LogicTick(); }
  }

  private void LogicTick() {
    foreach (var m in new List<MissionInstance>(Active)) {
      int shouldHaveRun = Mathf.Min(m.ticksPlanned, Mathf.FloorToInt((float)(System.DateTime.UtcNow - m.startUtc).TotalSeconds / m.tickS));
      for (int i = m.ticksRun; i < shouldHaveRun; i++) m.TickOnce(i);
      if (m.ticksRun >= m.ticksPlanned) Resolve(m);
    }
  }

  public void Resolve(MissionInstance m) {
    if (ServiceLocator.TryGet<EconomyService>(out var econ)) econ.Add(m.yieldsSoFar);
    Active.Remove(m);
  }

  public void SimulateOffline(double elapsedSeconds, int offlineCapSeconds = 8*60*60) {
    double sim = Mathf.Min((float)elapsedSeconds, offlineCapSeconds);
    foreach (var m in new List<MissionInstance>(Active)) {
      int shouldHaveRun = Mathf.Min(m.ticksPlanned, Mathf.FloorToInt(((float)sim + (float)(System.DateTime.UtcNow - m.startUtc).TotalSeconds) / m.tickS));
      for (int i = m.ticksRun; i < shouldHaveRun; i++) m.TickOnce(i);
      if (m.ticksRun >= m.ticksPlanned) Resolve(m);
    }
  }

  public MissionSnapshot[] ExportSnapshot() {
    var snapshot = new List<MissionSnapshot>(Active.Count);
    foreach (var m in Active) {
      snapshot.Add(new MissionSnapshot {
        missionId = m.missionId,
        type = m.type,
        tickS = m.tickS,
        ticksPlanned = m.ticksPlanned,
        ticksRun = m.ticksRun,
        startUtcTicks = m.startUtc.ToUniversalTime().Ticks,
        baseFail = m.baseFail,
        hazardMult = m.hazardMult,
        reliability = m.reliability,
        durationMult = m.durationMult,
        genericMitigation = m.genericMitigation,
        tagMitigationProduct = m.tagMitigationProduct,
        baseTickYield = m.baseTickYield,
        yieldsSoFar = m.yieldsSoFar,
        missionSeed = m.missionSeed
      });
    }
    return snapshot.ToArray();
  }

  public void ImportSnapshot(IEnumerable<MissionSnapshot> missions) {
    Active.Clear();
    if (missions == null) return;
    foreach (var m in missions) {
      var instance = new MissionInstance {
        missionId = m.missionId,
        type = m.type,
        tickS = m.tickS,
        ticksPlanned = m.ticksPlanned,
        ticksRun = m.ticksRun,
        startUtc = new DateTime(m.startUtcTicks, DateTimeKind.Utc),
        baseFail = m.baseFail,
        hazardMult = m.hazardMult,
        reliability = m.reliability,
        durationMult = m.durationMult,
        genericMitigation = m.genericMitigation,
        tagMitigationProduct = m.tagMitigationProduct,
        baseTickYield = m.baseTickYield,
        yieldsSoFar = m.yieldsSoFar,
        missionSeed = m.missionSeed
      };
      Active.Add(instance);
    }
  }
}
