using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class EventBus {

    public interface IMessage {}
    
    // Indexed by: EventType, SubscriptionId
    private static readonly Dictionary<string, Dictionary<string, Action<IMessage>>> AllListeners = new Dictionary<string, Dictionary<string, Action<IMessage>>>();
    
    // Indexed by: SubscriptionId
    // Speeds up subscriptions cleanup
    private static readonly Dictionary<string, HashSet<string>> OwnerSubscriptions = new Dictionary<string, HashSet<string>>();
    
    private static EventBus _instance = null;
    
    private static readonly object _lock = new object();
    
    private bool _listenersLock;
    
    private static string GetSubscriptionTag(Object obj) {
        return obj.GetInstanceID().ToString();
    }
    
    private static string EventKey<T>() where T : IMessage {
        return typeof(T).ToString();
    }

    public static EventBus Instance {
        get {
            lock (_lock) {
                return _instance ?? (_instance = new EventBus());
            }
        }
    }

    private EventBus() { }

    public void SendMessage<T>(T message) where T : IMessage {               
        // no one listening to this event yet
        if (!AllListeners.ContainsKey(EventKey<T>())) {
            return;
        }
        
        _listenersLock = true;
        
        foreach (var keyValuePair in AllListeners[EventKey<T>()]) {
            keyValuePair.Value(message);
        }

        _listenersLock = false;
    }

    public void Subscribe<T>(Object owner, Action<T> listener) where T : IMessage {
        if (_listenersLock) {
            Debug.LogWarning($"Cannot modify listeners for {typeof(T)} while event is resolving");
            return;
        }

        var eventKey = EventKey<T>();
        
        // initialise if needed
        if(!AllListeners.ContainsKey(eventKey))
            AllListeners.Add(eventKey, new Dictionary<string, Action<IMessage>>());

        var listeners = AllListeners[eventKey];
        var subscriptionTag = GetSubscriptionTag(owner);
        
        if (listeners.ContainsKey(subscriptionTag)) {
            Debug.LogWarning($"{subscriptionTag} already subscribed for event {typeof(T)}");
            return;
        }
        
        listeners.Add(
            subscriptionTag, 
            (message) => {
                var typedMessage = (T) message;
                listener(typedMessage);
            });

        // initialise if needed
        if(!OwnerSubscriptions.ContainsKey(subscriptionTag))
            OwnerSubscriptions.Add(subscriptionTag, new HashSet<string>());

        OwnerSubscriptions[subscriptionTag].Add(eventKey);
    }
    
    public void Unsubscribe<T>(Object owner) where T : IMessage {
        if (_listenersLock) {
            Debug.LogWarning($"Cannot modify listeners for {typeof(T)} while event is resolving");
            return;
        }

        var eventKey = EventKey<T>();
        if (!AllListeners.ContainsKey(eventKey))
            return;
        
        var listeners = AllListeners[eventKey];
        var subscriptionTag = GetSubscriptionTag(owner);

        listeners.Remove(subscriptionTag);

        OwnerSubscriptions[subscriptionTag].Remove(eventKey);
    }

    public void UnsubscribeAll(Object owner) {
        var subscriptionTag = GetSubscriptionTag(owner);
        
        if (!OwnerSubscriptions.ContainsKey(subscriptionTag))
            return;

        
        foreach (var eventId in OwnerSubscriptions[subscriptionTag]) {
            AllListeners[eventId].Remove(subscriptionTag);
        }

        OwnerSubscriptions[subscriptionTag].Clear();
    }
}
