using System;
using UnityEngine;

public class TimeKeeper : MonoBehaviour {
  public DateTime NowUtc => DateTime.UtcNow;
  public DateTime LastSeenUtc { get; private set; }
  public void MarkSeen() { LastSeenUtc = DateTime.UtcNow; }
  void Awake() { ServiceLocator.Register(this); MarkSeen(); }
}
