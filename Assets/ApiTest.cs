using UnityEngine;
using System.Collections;

public class ApiTest : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Auth.Login authLogin = new Auth.Login();
        authLogin.Exe();
    }
}
