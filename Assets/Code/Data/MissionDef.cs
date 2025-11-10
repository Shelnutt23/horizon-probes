using UnityEngine;
public enum MissionType { Survey, Mine, Replicate, Relay, Salvage, Stabilize }
[CreateAssetMenu(menuName="Defs/Mission")]
public class MissionDef : ScriptableObject {
  public string id;
  public MissionType type;
  public int baseDurationS;
  public int tickS;
  public float baseFail;
  public int baseM, baseV, baseD; public float baseX;
}
