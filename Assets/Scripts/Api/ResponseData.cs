using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class ResponseData<T>
{
    public T data;
    public string error_code;
    public int timestamp;
    public string version;
    public bool admin;
}


