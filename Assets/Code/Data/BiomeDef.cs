using UnityEngine;
[CreateAssetMenu(menuName="Defs/Biome")]
public class BiomeDef : ScriptableObject {
  public string id;
  public string displayName;
  public float hazardMult = 1f;
  public float yieldM = 1f, yieldV = 1f, yieldD = 1f, yieldX = 1f;
  public string[] tags;
}
