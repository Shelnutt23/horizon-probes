using UnityEngine;
public static class JsonUtil {
  public static T FromJson<T>(string json) => JsonUtility.FromJson<T>(json);
  public static string ToJson<T>(T obj, bool pretty=false) => JsonUtility.ToJson(obj, pretty);
}
