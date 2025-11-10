using UnityEngine;
public class EconomyService : MonoBehaviour {
  [SerializeField] int startingM = 25;
  [SerializeField] int startingV = 15;
  [SerializeField] int startingD = 20;
  [SerializeField] float startingX = 0f;

  public int M { get; private set; }
  public int V { get; private set; }
  public int D { get; private set; }
  public float X { get; private set; }

  void Awake(){
    ResetToStartingResources();
    ServiceLocator.Register(this);
  }

  void ResetToStartingResources(){
    M = startingM;
    V = startingV;
    D = startingD;
    X = startingX;
  }
  public void Add(ResourceBundle r){ M += r.M; V += r.V; D += r.D; X += r.X; }
  public bool Spend(int m=0, int v=0, int d=0){
    if (M < m || V < v || D < d) return false;
    M -= m; V -= v; D -= d; return true;
  }
  public EconomySnapshot ExportSnapshot(){
    return new EconomySnapshot { M = M, V = V, D = D, X = X };
  }
  public void ImportSnapshot(EconomySnapshot snapshot){
    if (snapshot == null) return;
    M = snapshot.M;
    V = snapshot.V;
    D = snapshot.D;
    X = snapshot.X;
  }
}
