using UnityEngine;
public class DemoCheats : MonoBehaviour {
  void Start(){
    if (ServiceLocator.TryGet<TechService>(out var tech)){
      tech.Unlock("t_isru");
      tech.Unlock("t_survey");
      tech.Unlock("t_reliab1");
    }
  }
}
