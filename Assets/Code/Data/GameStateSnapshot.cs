using System;
[Serializable]
public class GameStateSnapshot {
  public long savedAtUtcTicks;
  public EconomySnapshot economy;
  public ScanSnapshot scan;
  public MissionSnapshot[] missions = Array.Empty<MissionSnapshot>();
}

[Serializable]
public class EconomySnapshot {
  public int M;
  public int V;
  public int D;
  public float X;
}

[Serializable]
public class ScanSnapshot {
  public int Cap;
  public int Current;
  public float RegenIntervalS;
  public float Accumulator;
}

[Serializable]
public class MissionSnapshot {
  public string missionId;
  public MissionType type;
  public int tickS;
  public int ticksPlanned;
  public int ticksRun;
  public long startUtcTicks;
  public float baseFail;
  public float hazardMult;
  public float reliability;
  public float durationMult;
  public float genericMitigation;
  public float tagMitigationProduct;
  public ResourceBundle baseTickYield;
  public ResourceBundle yieldsSoFar;
  public ulong missionSeed;
}
