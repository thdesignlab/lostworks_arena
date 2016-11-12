using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class ResponseData<T>
{
    public T data { get; set; }
    public string error_code;
    public int timestamp;
    public string version;
    public bool admin;
}
