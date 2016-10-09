using UnityEngine;
using System.Collections;

public class ApiTest : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        UserAccount.Model model = new UserAccount.Model();
        model.Get();
    }
}
