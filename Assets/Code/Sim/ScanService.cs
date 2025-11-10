using UnityEngine;
public class ScanService : MonoBehaviour {
  public int Cap = 30;
  public float RegenIntervalS = 8f;
  public int Current { get; private set; }
  float _accum;
  void Awake(){ ServiceLocator.Register(this); }
  void Start(){ Current = Cap; }
  void Update(){
    _accum += Time.deltaTime;
    if (_accum >= RegenIntervalS){
      _accum -= RegenIntervalS;
      Current = Mathf.Min(Current + 1, Cap);
    }
  }
  public bool Consume(int amount){
    if (Current < amount) return false;
    Current -= amount; return true;
  }
  public ScanSnapshot ExportSnapshot(){
    return new ScanSnapshot { Cap = Cap, Current = Current, RegenIntervalS = RegenIntervalS, Accumulator = _accum };
  }
  public void ImportSnapshot(ScanSnapshot snapshot){
    if (snapshot == null) return;
    Cap = snapshot.Cap;
    Current = Mathf.Clamp(snapshot.Current, 0, Cap);
    RegenIntervalS = snapshot.RegenIntervalS;
    _accum = Mathf.Clamp(snapshot.Accumulator, 0f, RegenIntervalS);
  }
}
