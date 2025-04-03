using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using System;
using System.Text;
using System.Reflection;
using Whisper;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class JsonConverter<T> where T : class, new()
{

      public static List<T>? DeserializeToList(string source)
    {
        if (source == null)
        {
            return null;
        }
        return JsonConvert.DeserializeObject<List<T>>(source);
    }

    public static T? Deserialize(string source)
    {
        if (source == null)
        {
            return null;
        }
        return JsonConvert.DeserializeObject<T>(source);
    }
}

