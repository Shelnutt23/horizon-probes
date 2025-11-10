using System;
public static class RNG {
  public static ulong SplitMix64(ref ulong state) {
    ulong z = (state += 0x9E3779B97F4A7C15UL);
    z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
    z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
    return z ^ (z >> 31);
  }
  public static float Next01(ref ulong state) => (SplitMix64(ref state) >> 11) * (1.0f / (1UL << 53));
  public static ulong Seed(params ulong[] parts) {
    ulong s = 0x12345678ABCDEF01UL;
    foreach (var p in parts) { s ^= p + 0x9E3779B97F4A7C15UL + (s << 6) + (s >> 2); }
    return s;
  }
}
