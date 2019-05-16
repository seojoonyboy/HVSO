using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnEvent(Enum Event_Type, Component Sender, object Param = null);

public interface IEventHanlder {
    void AddListener(Enum Event_Type, OnEvent Listener);
    void PostNotification(Enum Event_Type, Component Sender, object Param = null);
    void RemoveListener(Enum Event_Type, OnEvent Listener);
    void RemoveEvent(Enum Event_Type);
}
