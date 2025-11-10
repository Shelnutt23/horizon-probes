using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Node { public int id; public Vector2 pos; public string biomeId = "b_asteroid"; }
[System.Serializable] public class Edge { public int a, b; public float distance; }

public class Sector {
  public List<Node> nodes = new List<Node>();
  public List<Edge> edges = new List<Edge>();
}

public static class SectorGenerator {
  public static Sector Generate(int nodeCount, ulong seed) {
    var sector = new Sector();
    var nodes = sector.nodes;
    var edges = sector.edges;

    ulong s = seed;
    for (int i = 0; i < nodeCount; i++) {
      float x = RNG.Next01(ref s);
      float y = RNG.Next01(ref s);
      nodes.Add(new Node { id = i, pos = new Vector2(x, y) });
    }
    for (int i = 0; i < nodeCount; i++) {
      var dists = new List<(int j, float d)>();
      for (int j = 0; j < nodeCount; j++) if (i != j) {
        float d = Vector2.Distance(nodes[i].pos, nodes[j].pos);
        dists.Add((j, d));
      }
      dists.Sort((p, q) => p.d.CompareTo(q.d));
      for (int k = 0; k < 3; k++) {
        var j = dists[k].j;
        if (i < j) edges.Add(new Edge { a = i, b = j, distance = dists[k].d });
      }
    }
    return sector;
  }
}
