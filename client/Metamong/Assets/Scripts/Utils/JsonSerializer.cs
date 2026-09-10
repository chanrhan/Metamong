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

public static class JsonConverter
{
    public static List<T> DeserializeToList<T>(string source)
    {
        if (source == null)
        {
            return null;
        }
        return JsonConvert.DeserializeObject<List<T>>(source);
    }

    public static T Deserialize<T>(string source)
    {
        if (source == null)
        {
            return default;
        }
        return JsonConvert.DeserializeObject<T>(source);
    }
}

