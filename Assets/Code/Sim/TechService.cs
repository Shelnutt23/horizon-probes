using System.Collections.Generic;
using UnityEngine;

public class TechService : MonoBehaviour {
  public readonly HashSet<string> Unlocked = new HashSet<string>();
  void Awake(){ ServiceLocator.Register(this); }
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
}
