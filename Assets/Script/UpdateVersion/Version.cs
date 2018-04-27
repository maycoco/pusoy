using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class VersionInfo
{
    public static readonly string version = "0.0.9";
    public static readonly int version_code = 9;
}

public class Version : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Text>().text = VersionInfo.version;
	}
}
