using UnityEngine;
using System.Collections;
using System;

public class DebugText : MonoBehaviour
{

    public static DebugText debugLogText;

    void Awake()
    {
        debugLogText = this;
    }

    public static void LOG(string msg)
    {
        debugLogText.GetComponent<TextMesh>().text += "\n " + msg;
    }
}