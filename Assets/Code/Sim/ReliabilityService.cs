public static class ReliabilityService {
  public static float FailProbTick(float baseFail, float hazardMult, float durationMult, float reliability, float genericMitigation, float tagMitigationProduct) {
    return baseFail * hazardMult * durationMult * (1f - reliability) * (1f - genericMitigation) * tagMitigationProduct;
  }
}
