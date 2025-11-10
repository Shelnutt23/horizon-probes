using System;
using System.Collections.Generic;

public static class ServiceLocator {
  private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
  public static void Register<T>(T instance) where T : class { _services[typeof(T)] = instance; }
  public static T Get<T>() where T : class { return _services.ContainsKey(typeof(T)) ? _services[typeof(T)] as T : null; }
  public static bool TryGet<T>(out T instance) where T : class {
    if (_services.TryGetValue(typeof(T), out var obj)) { instance = obj as T; return true; }
    instance = null; return false;
  }
  public static void Clear() => _services.Clear();
}
