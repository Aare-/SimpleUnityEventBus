using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class EventBus {

    private static EventBusInstance _instance = null;

    private static readonly object _lock = new object();

    public static EventBusInstance Instance {
        get {
            lock (_lock) {
                return _instance ?? (_instance = new EventBusInstance());
            }
        }
    }

    private EventBus() { }
}