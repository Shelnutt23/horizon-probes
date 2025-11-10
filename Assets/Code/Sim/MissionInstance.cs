using System;
using System.Collections.Generic;
public class MissionInstance {
  public string missionId;
  public MissionType type;
  public int tickS;
  public int ticksPlanned;
  public int ticksRun;
  public DateTime startUtc;
  public float baseFail;
  public float hazardMult = 1f;
  public float reliability = 0.8f;
  public float durationMult = 1f;
  public float genericMitigation = 0f;
  public float tagMitigationProduct = 1f;
  public ResourceBundle baseTickYield;
  public ulong missionSeed;
  public ResourceBundle yieldsSoFar;
  public LoadoutBuilder.LoadoutStats loadoutStats;
  public string chassisId;
  public List<string> moduleIds = new List<string>();

  public bool TickOnce(int tickIndex) {
    var state = RNG.Seed(missionSeed, (ulong)tickIndex);
    float p = ReliabilityService.FailProbTick(baseFail, hazardMult, durationMult, reliability, genericMitigation, tagMitigationProduct);
    float roll = RNG.Next01(ref state);
    bool failed = roll < p;
    if (!failed) yieldsSoFar += baseTickYield;
    ticksRun++;
    return failed;
  }
}
