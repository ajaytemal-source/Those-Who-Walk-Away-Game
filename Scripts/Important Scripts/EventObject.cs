using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class GameEvent<T>
{
    private event Action<T> listeners;

    public void Raise(T value)
    {
        listeners?.Invoke(value);
    }

    public void Subscribe(Action<T> listener)
    {
        listeners += listener;
    }

    public void Unsubscribe(Action<T> listener)
    {
        listeners -= listener;
    }
}


public static class GameEvents
{
    //For Main Game Path Triggers
    public static GameEvent<int> FileDownloaded = new GameEvent<int>();
    public static GameEvent<Trigger> TriggerEvent = new GameEvent<Trigger>();
    public static GameEvent<int> FileInSpecificFolder = new GameEvent<int>(); 
    public static GameEvent<int> ConversationComplete = new GameEvent<int>();
    public static GameEvent<int> FileSent = new GameEvent<int>(); 

    //Dialogue Stalls: 
    public static GameEvent<ConversationStall> StartStall = new GameEvent<ConversationStall>();
    public static GameEvent<ConversationStall> EndStall = new GameEvent<ConversationStall>();

    //Notification -> Pressed -> Application Opens/Triggers
    public static GameEvent<int> EmailNotifPressed = new GameEvent<int>(); 
    public static GameEvent<int> FinishDwnldNotifPressed = new GameEvent<int>(); 
    public static GameEvent<Thread> TeamsNotifPressed = new GameEvent<Thread>();

    //Terminal Triggers 
    public static GameEvent<TerminalTrigger> TerminalCommandEvent = new GameEvent<TerminalTrigger>();

    //Used in Phase 6, so both scripts can respond to choices from the other 
    public static GameEvent<int> SendChoiceToIRYS = new GameEvent<int>();
    public static GameEvent<int> SendChoiceToTeams = new GameEvent<int>();

    //Used in Phase 6 to triggerGlitches 
    public static GameEvent<int> TriggerGlitchReciever = new GameEvent<int>(); 
}

