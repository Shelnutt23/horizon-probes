using System;
using UnityEngine;
[Serializable] public struct StatEffect { public string stat; public string op; public float value; public string resource; public string tag; }
[Serializable] public struct RiskEffect { public string tag; public float value; }
[CreateAssetMenu(menuName="Defs/Module")]
public class ModuleDef : ScriptableObject {
  public string id;
  public string slot;
  public StatEffect[] effects;
  public RiskEffect[] risks;
}
