using UnityEngine;
[CreateAssetMenu(menuName="Defs/ProbeChassis")]
public class ProbeChassisDef : ScriptableObject {
  public string id;
  public int baseSlots;
  public float autonomy, reliability, throughput, stealth;
  public string role;
}
