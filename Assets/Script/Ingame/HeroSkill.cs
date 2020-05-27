using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SocketFormat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class HeroSkill {
    public void Activate(bool isPlayer, string heroId, List<JToken> toList, string trigger, DequeueCallback callback) {
        callback();
    }
}