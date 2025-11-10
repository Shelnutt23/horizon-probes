using System;
using UnityEngine;

[Serializable] public class BiomeList { public BiomeJson[] biomes; }
[Serializable] public class BiomeJson { public string id; public string displayName; public float hazardMult; public float yieldM, yieldV, yieldD, yieldX; public string[] tags; }

[Serializable] public class ChassisList { public ChassisJson[] chassis; }
[Serializable] public class ChassisJson { public string id; public int baseSlots; public float autonomy, reliability, throughput, stealth; public string role; }

[Serializable] public class ModuleList { public ModuleJson[] modules; }
[Serializable] public class ModuleJson { public string id; public string slot; public StatEffect[] effects; public RiskEffect[] risks; }

[Serializable] public class MissionList { public MissionJson[] missions; }
[Serializable] public class MissionJson { public string id; public string type; public int baseDurationS; public int tickS; public float baseFail; public int baseM, baseV, baseD; public float baseX; }

[Serializable] public class TechList { public TechJson[] tech; }
[Serializable] public class TechJson { public string id; public int tier; public int cost_D; public TechEffect[] effects; }
[Serializable] public class TechEffect { public string target; public string id; public string key; public string mission; public string resource; public string op; public float value; }

public static class ContentLoader {
  public static BiomeList Biomes => Load<BiomeList>("Content/Biomes");
  public static ChassisList Chassis => Load<ChassisList>("Content/Chassis");
  public static ModuleList Modules => Load<ModuleList>("Content/Modules");
  public static MissionList Missions => Load<MissionList>("Content/Missions");
  public static TechList Tech => Load<TechList>("Content/Tech");
  private static T Load<T>(string path) {
    var text = Resources.Load<TextAsset>(path);
    if (text == null) { Debug.LogError("Missing content at Resources/" + path); return default; }
    return JsonUtility.FromJson<T>(text.text);
  }
}
