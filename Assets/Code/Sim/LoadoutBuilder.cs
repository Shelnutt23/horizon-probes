using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadoutBuilder : MonoBehaviour {
  [Serializable]
  public class LoadoutConfiguration {
    public ChassisJson Chassis;
    public ModuleJson PayloadModule;
    public ModuleJson PowerModule;
    public List<ModuleJson> UtilityModules = new List<ModuleJson>();
  }

  public class LoadoutStats {
    public ChassisJson Chassis;
    public float Autonomy;
    public float Reliability;
    public float Throughput;
    public float Stealth;
    public float CargoDensity = 1f;
    public float EnergyOutput;
    public float GenericMitigationBase;
    public float GenericMitigationMultiplier = 1f;
    public float GenericMitigationAdd;
    public readonly HashSet<string> UnlockTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public readonly HashSet<string> FailureTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, float> _resourceMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, float> _tagMitigations = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

    public float GetResourceMultiplier(string resource) {
      if (string.IsNullOrEmpty(resource)) return 1f;
      return _resourceMultipliers.TryGetValue(resource, out var value) ? value : 1f;
    }

    public void ApplyResourceMultiplier(string resource, string op, float value) {
      if (string.IsNullOrEmpty(resource)) return;
      op = string.IsNullOrEmpty(op) ? "mul" : op.ToLowerInvariant();
      float current = _resourceMultipliers.TryGetValue(resource, out var existing) ? existing : 1f;
      _resourceMultipliers[resource] = ApplyOperation(current, op, value, 1f);
    }

    public void ApplyTagMitigation(string tag, string op, float value) {
      if (string.IsNullOrEmpty(tag)) return;
      op = string.IsNullOrEmpty(op) ? "mul" : op.ToLowerInvariant();
      float current = _tagMitigations.TryGetValue(tag, out var existing) ? existing : 1f;
      _tagMitigations[tag] = ApplyOperation(current, op, value, 1f);
    }

    public float GetTagMitigationProduct() {
      float product = 1f;
      foreach (var value in _tagMitigations.Values) product *= value;
      return product;
    }

    static float ApplyOperation(float current, string op, float value, float defaultValue) {
      switch (op) {
        case "set": return value;
        case "mul": return current * value;
        case "add": return current + value;
        default: return current;
      }
    }
  }

  ChassisJson[] _chassis = Array.Empty<ChassisJson>();
  ModuleJson[] _modules = Array.Empty<ModuleJson>();
  Dictionary<string, ModuleJson> _modulesById = new Dictionary<string, ModuleJson>(StringComparer.OrdinalIgnoreCase);
  Dictionary<string, List<ModuleJson>> _modulesBySlot = new Dictionary<string, List<ModuleJson>>(StringComparer.OrdinalIgnoreCase);

  void Awake() {
    ServiceLocator.Register(this);
    var chassisList = ContentLoader.Chassis;
    if (chassisList?.chassis != null) _chassis = chassisList.chassis;
    var moduleList = ContentLoader.Modules;
    if (moduleList?.modules != null) _modules = moduleList.modules;
    _modulesById = _modules.ToDictionary(m => m.id, m => m, StringComparer.OrdinalIgnoreCase);
    _modulesBySlot = _modules
      .GroupBy(m => m.slot ?? string.Empty, StringComparer.OrdinalIgnoreCase)
      .ToDictionary(g => g.Key, g => g.OrderBy(m => m.id, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);
  }

  public IReadOnlyList<ChassisJson> AvailableChassis => _chassis;

  public IReadOnlyList<ModuleJson> GetModulesForSlot(string slot) {
    if (slot == null) slot = string.Empty;
    return _modulesBySlot.TryGetValue(slot, out var list) ? list : Array.Empty<ModuleJson>();
  }

  public ModuleJson GetModule(string moduleId) {
    if (string.IsNullOrEmpty(moduleId)) return null;
    return _modulesById.TryGetValue(moduleId, out var module) ? module : null;
  }

  public static MissionType ParseMissionType(string missionType) {
    if (Enum.TryParse(missionType, true, out MissionType parsed)) return parsed;
    return MissionType.Survey;
  }

  public int GetUtilitySlotCount(ChassisJson chassis) => Mathf.Max(0, chassis?.baseSlots ?? 0);

  public void EnsureUtilitySlotSize(LoadoutConfiguration config) {
    if (config == null) return;
    if (config.UtilityModules == null) config.UtilityModules = new List<ModuleJson>();
    int desired = GetUtilitySlotCount(config.Chassis);
    while (config.UtilityModules.Count < desired) config.UtilityModules.Add(null);
    while (config.UtilityModules.Count > desired && config.UtilityModules.Count > 0) config.UtilityModules.RemoveAt(config.UtilityModules.Count - 1);
  }

  public LoadoutStats CalculateStats(LoadoutConfiguration config) {
    if (config?.Chassis == null) return null;
    EnsureUtilitySlotSize(config);
    var stats = new LoadoutStats {
      Chassis = config.Chassis,
      Autonomy = config.Chassis.autonomy,
      Reliability = config.Chassis.reliability,
      Throughput = config.Chassis.throughput,
      Stealth = config.Chassis.stealth
    };
    foreach (var module in EnumerateModules(config)) {
      if (module?.effects == null) continue;
      foreach (var effect in module.effects) ApplyStatEffect(stats, effect);
    }
    return stats;
  }

  IEnumerable<ModuleJson> EnumerateModules(LoadoutConfiguration config) {
    if (config.PayloadModule != null) yield return config.PayloadModule;
    if (config.PowerModule != null) yield return config.PowerModule;
    if (config.UtilityModules != null) {
      foreach (var mod in config.UtilityModules) if (mod != null) yield return mod;
    }
  }

  void ApplyStatEffect(LoadoutStats stats, StatEffect effect) {
    if (stats == null) return;
    string op = string.IsNullOrEmpty(effect.op) ? "add" : effect.op.ToLowerInvariant();
    string statKey = string.IsNullOrEmpty(effect.stat) ? string.Empty : effect.stat.ToLowerInvariant();
    switch (statKey) {
      case "autonomy":
        stats.Autonomy = ApplyNumeric(stats.Autonomy, op, effect.value, 1f);
        break;
      case "reliability":
        stats.Reliability = ApplyNumeric(stats.Reliability, op, effect.value, 1f);
        break;
      case "throughput":
        stats.Throughput = ApplyNumeric(stats.Throughput, op, effect.value, 1f);
        break;
      case "stealth":
        stats.Stealth = ApplyNumeric(stats.Stealth, op, effect.value, 1f);
        break;
      case "genericmitigation":
        ApplyGenericMitigation(stats, op, effect.value);
        break;
      case "mitigation":
        stats.ApplyTagMitigation(effect.tag, op, effect.value);
        break;
      case "energyoutput":
        stats.EnergyOutput = ApplyNumeric(stats.EnergyOutput, op, effect.value, 0f);
        break;
      case "cargodensity":
        stats.CargoDensity = ApplyNumeric(stats.CargoDensity, op, effect.value, 1f);
        break;
      case "failuretag":
        if (!string.IsNullOrEmpty(effect.tag)) stats.FailureTags.Add(effect.tag);
        break;
      case "unlock":
        if (!string.IsNullOrEmpty(effect.tag)) stats.UnlockTags.Add(effect.tag);
        break;
    }
    if (!string.IsNullOrEmpty(effect.resource)) stats.ApplyResourceMultiplier(effect.resource, op, effect.value);
  }

  static float ApplyNumeric(float current, string op, float value, float defaultValue) {
    switch (op) {
      case "set": return value;
      case "mul": return current * value;
      case "add": return current + value;
      default: return current;
    }
  }

  static void ApplyGenericMitigation(LoadoutStats stats, string op, float value) {
    switch (op) {
      case "set":
        stats.GenericMitigationBase = value;
        break;
      case "mul":
        stats.GenericMitigationMultiplier *= value;
        break;
      case "add":
        stats.GenericMitigationAdd += value;
        break;
    }
  }

  public List<string> ValidateConfiguration(LoadoutConfiguration config) {
    var issues = new List<string>();
    if (config == null) {
      issues.Add("No loadout configured.");
      return issues;
    }
    if (config.Chassis == null) issues.Add("Select a chassis.");
    if (config.PayloadModule == null) issues.Add("Select a payload module.");
    if (config.PowerModule == null) issues.Add("Select a power module.");
    int requiredUtilities = GetUtilitySlotCount(config.Chassis);
    int assignedUtilities = config.UtilityModules?.Count(m => m != null) ?? 0;
    if (requiredUtilities > 0 && assignedUtilities < requiredUtilities)
      issues.Add($"Assign {requiredUtilities} utility module(s).");
    return issues;
  }

  public bool TryCreateMissionInstance(MissionJson mission, LoadoutConfiguration config, out MissionInstance instance, out string error) {
    instance = null;
    error = null;
    if (mission == null) {
      error = "No mission selected.";
      return false;
    }
    var issues = ValidateConfiguration(config);
    if (issues.Count > 0) {
      error = issues[0];
      return false;
    }
    var stats = CalculateStats(config);
    if (stats == null) {
      error = "Unable to compute loadout stats.";
      return false;
    }
    MissionType missionType = ParseMissionType(mission.type);
    if (ServiceLocator.TryGet<TechService>(out var tech)) tech.ApplyTechToLoadout(stats, missionType);
    instance = BuildMissionInstance(mission, config, stats, missionType);
    return true;
  }

  MissionInstance BuildMissionInstance(MissionJson mission, LoadoutConfiguration config, LoadoutStats stats, MissionType missionType) {
    var instance = new MissionInstance {
      missionId = mission.id,
      type = missionType,
      tickS = Mathf.Max(1, mission.tickS),
      ticksPlanned = Mathf.Max(1, Mathf.RoundToInt(mission.baseDurationS / Mathf.Max(1f, mission.tickS))),
      startUtc = DateTime.UtcNow,
      baseFail = mission.baseFail,
      reliability = Mathf.Clamp01(stats.Reliability),
      baseTickYield = new ResourceBundle { M = mission.baseM, V = mission.baseV, D = mission.baseD, X = mission.baseX },
      missionSeed = RNG.Seed((ulong)DateTime.UtcNow.Ticks, (ulong)UnityEngine.Random.Range(0, int.MaxValue)),
      yieldsSoFar = new ResourceBundle(),
      loadoutStats = stats,
      chassisId = config.Chassis?.id,
      moduleIds = CollectModuleIds(config)
    };
    ApplyYieldModifiers(instance, stats);
    instance.genericMitigation = EvaluateGenericMitigation(stats);
    instance.tagMitigationProduct *= stats.GetTagMitigationProduct();
    return instance;
  }

  static List<string> CollectModuleIds(LoadoutConfiguration config) {
    var list = new List<string>();
    if (config.PayloadModule != null) list.Add(config.PayloadModule.id);
    if (config.PowerModule != null) list.Add(config.PowerModule.id);
    if (config.UtilityModules != null) {
      foreach (var mod in config.UtilityModules) if (mod != null) list.Add(mod.id);
    }
    return list;
  }

  void ApplyYieldModifiers(MissionInstance instance, LoadoutStats stats) {
    if (instance == null || stats == null) return;
    float throughput = Mathf.Max(0f, stats.Throughput);
    instance.baseTickYield.M = Mathf.RoundToInt(instance.baseTickYield.M * throughput * stats.GetResourceMultiplier("M"));
    instance.baseTickYield.V = Mathf.RoundToInt(instance.baseTickYield.V * throughput * stats.GetResourceMultiplier("V"));
    instance.baseTickYield.D = Mathf.RoundToInt(instance.baseTickYield.D * throughput * stats.GetResourceMultiplier("D"));
    instance.baseTickYield.X = instance.baseTickYield.X * throughput * stats.GetResourceMultiplier("X");
    instance.baseTickYield.M = Mathf.RoundToInt(instance.baseTickYield.M * stats.CargoDensity);
    instance.baseTickYield.V = Mathf.RoundToInt(instance.baseTickYield.V * stats.CargoDensity);
    instance.baseTickYield.D = Mathf.RoundToInt(instance.baseTickYield.D * stats.CargoDensity);
  }

  public float EvaluateGenericMitigation(LoadoutStats stats) {
    if (stats == null) return 0f;
    float mitigation = Mathf.Clamp01(stats.GenericMitigationBase);
    mitigation = 1f - (1f - mitigation) * Mathf.Max(0f, stats.GenericMitigationMultiplier);
    mitigation += stats.GenericMitigationAdd;
    return Mathf.Clamp01(mitigation);
  }
}
