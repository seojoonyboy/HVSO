using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace dataModules {
    //TODO : HTTP Pro의 LitJson과 통합
    public class JsonReader {
        public static T Read<T>(string data) {
            return (T)Convert.ChangeType(JsonConvert.DeserializeObject<T>(data), typeof(T));
        }
    }
}