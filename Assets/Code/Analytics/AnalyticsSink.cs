using System.IO;
using UnityEngine;

public static class AnalyticsSink {
  private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "analytics.ldjson");
  public static void Emit(string eventName, string jsonPayload) {
    try {
      File.AppendAllText(Path, $"{{\"t\":\"{System.DateTime.UtcNow:o}\",\"e\":\"{eventName}\",\"p\":{jsonPayload}}}\n");
    } catch { }
  }
}
