using UnityEngine;
public class HUDController : MonoBehaviour {
  EconomyService econ;
  void Start() { ServiceLocator.TryGet<EconomyService>(out econ); }
  void OnGUI() {
    if (econ == null) return;
    GUILayout.BeginArea(new Rect(10, 10, 360, 150), GUI.skin.box);
    GUILayout.Label($"M: {econ.M}  V: {econ.V}  D: {econ.D}  X: {econ.X:F1}");
    if (ServiceLocator.TryGet<ScanService>(out var scan)) GUILayout.Label($"Scan: {scan.Current}/{scan.Cap} (+1/8s)");
    GUILayout.Label("Unity 6.2 prototype build");
    GUILayout.EndArea();
  }
}
