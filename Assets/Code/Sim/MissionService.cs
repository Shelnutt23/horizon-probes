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
}
