using UnityEngine;
public class GameBootstrap : MonoBehaviour {
  void Awake(){
    if (ServiceLocator.Get<TimeKeeper>() == null) gameObject.AddComponent<TimeKeeper>();
    if (ServiceLocator.Get<MissionService>() == null) gameObject.AddComponent<MissionService>();
    if (ServiceLocator.Get<EconomyService>() == null) gameObject.AddComponent<EconomyService>();
    if (ServiceLocator.Get<TechService>() == null) gameObject.AddComponent<TechService>();
    if (ServiceLocator.Get<ScanService>() == null) gameObject.AddComponent<ScanService>();
    if (GetComponent<SaveManager>() == null) gameObject.AddComponent<SaveManager>();
  }
}
