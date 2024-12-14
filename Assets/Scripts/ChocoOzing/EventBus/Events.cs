using UnityEngine;

public interface IEvent { }

public struct TestEvent : IEvent
{
    
}

public struct PlayerEvent :IEvent
{
   public GameObject player;
}

public class Events : IEvent
{
    
}
