using System.Text;
using UnityEngine;
using System.IO;

public static class SaveSystem {
  private static string PathFor(string slot) => System.IO.Path.Combine(Application.persistentDataPath, slot + ".json");
  private static byte[] Xor(byte[] data) {
    byte key = 0x5A; for (int i=0;i<data.Length;i++) data[i]^=key; return data;
  }
  public static void Save(string slot, string json) {
    var bytes = Xor(Encoding.UTF8.GetBytes(json));
    File.WriteAllBytes(PathFor(slot), bytes);
  }
  public static string Load(string slot) {
    var p = PathFor(slot);
    if (!File.Exists(p)) return null;
    var bytes = File.ReadAllBytes(p);
    bytes = Xor(bytes);
    return Encoding.UTF8.GetString(bytes);
  }
}
