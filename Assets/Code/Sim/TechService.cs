using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TechService : MonoBehaviour {
  public readonly HashSet<string> Unlocked = new HashSet<string>();
  Dictionary<string, TechJson> _techById = new Dictionary<string, TechJson>(StringComparer.OrdinalIgnoreCase);

  void Awake(){
    ServiceLocator.Register(this);
    var techList = ContentLoader.Tech;
    if (techList?.tech != null) {
      _techById = techList.tech.ToDictionary(t => t.id, t => t, StringComparer.OrdinalIgnoreCase);
    }
  }
  public bool Has(string techId) => Unlocked.Contains(techId);
  public void Unlock(string techId){ Unlocked.Add(techId); }
  public void ApplyTechToMission(MissionInstance m){
    if (Has("t_reliab1")) m.genericMitigation = Mathf.Max(m.genericMitigation, 0.10f);
    if (Has("t_isru") && (m.baseTickYield.M > 0 || m.baseTickYield.V > 0)){
      m.baseTickYield.M = Mathf.RoundToInt(m.baseTickYield.M * 1.10f);
      m.baseTickYield.V = Mathf.RoundToInt(m.baseTickYield.V * 1.10f);
    }
    if (Has("t_survey") && m.type == MissionType.Survey){
      m.baseTickYield.D = Mathf.RoundToInt(m.baseTickYield.D * 1.20f);
    }
  }

  public void ApplyTechToLoadout(LoadoutBuilder.LoadoutStats stats, MissionType missionType) {
    if (stats == null) return;
    foreach (var techId in Unlocked) {
      if (!_techById.TryGetValue(techId, out var tech) || tech.effects == null) continue;
      foreach (var effect in tech.effects) ApplyEffectToLoadout(stats, missionType, effect);
    }
  }

  void ApplyEffectToLoadout(LoadoutBuilder.LoadoutStats stats, MissionType missionType, TechEffect effect) {
    if (effect == null) return;
    string target = effect.target ?? string.Empty;
    switch (target) {
      case "yield":
        if (!string.IsNullOrEmpty(effect.mission) && !string.Equals(effect.mission, missionType.ToString(), StringComparison.OrdinalIgnoreCase)) return;
        stats.ApplyResourceMultiplier(effect.resource, effect.op, effect.value);
        break;
      case "unlock":
        if (!string.IsNullOrEmpty(effect.id)) stats.UnlockTags.Add(effect.id);
        break;
      case "fail":
        if (!string.IsNullOrEmpty(effect.key) && effect.key.Equals("genericMitigation", StringComparison.OrdinalIgnoreCase)) {
          string op = string.IsNullOrEmpty(effect.op) ? "mul" : effect.op.ToLowerInvariant();
          switch (op) {
            case "mul": stats.GenericMitigationMultiplier *= effect.value; break;
            case "add": stats.GenericMitigationAdd += effect.value; break;
            case "set": stats.GenericMitigationBase = effect.value; break;
          }
        }
        break;
      case "stat":
        ApplyStatEffectToLoadout(stats, effect);
        break;
    }
  }

  void ApplyStatEffectToLoadout(LoadoutBuilder.LoadoutStats stats, TechEffect effect) {
    if (stats == null || effect == null || string.IsNullOrEmpty(effect.key)) return;
    string key = effect.key.ToLowerInvariant();
    string op = string.IsNullOrEmpty(effect.op) ? "mul" : effect.op.ToLowerInvariant();
    float value = effect.value;
    switch (key) {
      case "autonomy":
        stats.Autonomy = ApplyNumeric(stats.Autonomy, op, value);
        break;
      case "reliability":
        stats.Reliability = ApplyNumeric(stats.Reliability, op, value);
        break;
      case "throughput":
        stats.Throughput = ApplyNumeric(stats.Throughput, op, value);
        break;
      case "stealth":
        stats.Stealth = ApplyNumeric(stats.Stealth, op, value);
        break;
      case "genericmitigation":
        switch (op) {
          case "mul": stats.GenericMitigationMultiplier *= value; break;
          case "add": stats.GenericMitigationAdd += value; break;
          case "set": stats.GenericMitigationBase = value; break;
        }
        break;
    }
  }

  static float ApplyNumeric(float current, string op, float value) {
    switch (op) {
      case "set": return value;
      case "mul": return current * value;
      case "add": return current + value;
      default: return current;
    }
  }
}
