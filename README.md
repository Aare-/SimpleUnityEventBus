# SimpleUnityEventBus
Very simple and robust implementation of event bus for unity
# Sample Usage
Event bus allows you to subscribe, unsubscribe and send messages betwween looselly coupled objects.
Here are examples on how to achieve this.

## Message definition

```C#
  public struct SimpleEventNoData : EventBus.IMessage {}

  public struct SimpleEvent : EventBus.IMessage {
      public string AnyData;
  }
```

## Sending message

```C#
  EventBus
    .Instance
    .SendMessage(new Msg.SimpleEventNoData() {});
    
  EventBus
    .Instance
    .SendMessage(new Msg.SimpleEvent() {
      AnyData = "Test Data"
    });
```

## Subscribing to event
```C#
  EventBus
    .Instance
    .Subscribe<Msg.SimpleEventNoData>(
      this, // this must extend Unity.Object
      message => {
        // on event
      });
```

## Unsubscribing from event

```C#
  EventBus
    .Instance
    .Unsubscribe<Msg.SimpleEventNoData>(this);
```

## Unsubscribing from all events

```C#
  EventBus
    .Instance
    .UnsubscribeAll(this);
```

