using UnityEngine;
public class EconomyService : MonoBehaviour {
  public int M { get; private set; }
  public int V { get; private set; }
  public int D { get; private set; }
  public float X { get; private set; }
  void Awake(){ ServiceLocator.Register(this); }
  public void Add(ResourceBundle r){ M += r.M; V += r.V; D += r.D; X += r.X; }
  public bool Spend(int m=0, int v=0, int d=0){
    if (M < m || V < v || D < d) return false;
    M -= m; V -= v; D -= d; return true;
  }
}
