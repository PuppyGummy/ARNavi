using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPrefsForFirst : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("firstTime") != 1) {
            PlayerPrefs.SetInt("firstTime", 1);
            SceneManager.LoadScene("Get Started");
        }
    }
}
