using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class VideoManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer openingVideo;

    //Start is called before the first frame update
    void Awake()
    {
        openingVideo.loopPointReached += CheckOver;
    }
    //Checks to see when the animation is finished
    public void CheckOver(UnityEngine.Video.VideoPlayer vp)
    {
        OnMovieEnded();
    }
    private void OnMovieEnded()
    {
        //if it is the user's first time on the app, send them to the langauge selection screen
        if (PlayerPrefs.GetInt("firstTime") != 1)
        {
            PlayerPrefs.SetInt("firstTime", 1);
            NavigationButtons.ToSelectLanguage();
        }
        //if it is not the user's first time, send them to the main menu
        else
        {
            NavigationButtons.ToMainMenu();
        }
    }
}
