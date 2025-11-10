[System.Serializable]
public struct ResourceBundle {
  public int M, V, D; public float X;
  public static ResourceBundle operator +(ResourceBundle a, ResourceBundle b)
    => new ResourceBundle { M = a.M + b.M, V = a.V + b.V, D = a.D + b.D, X = a.X + b.X };
}
