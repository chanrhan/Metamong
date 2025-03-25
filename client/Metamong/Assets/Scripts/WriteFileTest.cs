using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

public class WriteFileTest : MonoBehaviour
{
    public Text text;
    public Button button;

    public int count = 0;


    private void Start()
    {
        button.onClick.AddListener(WriteFile);
    }

    private void WriteFile()
    {
        if(count == 1)
        {
            Debug.Log("Writing to file...  ");
            string result = text.text;
            File.WriteAllTextAsync("full_sentence.txt", result);
        }
        count++;
    }
}
