using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class MusicStart : MonoBehaviour
{
    private static MusicStart instance { get; set; }

    public string[] tune;

    public FMOD.Studio.EventInstance musicEvent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }    
        //else
        {
            //Destroy(gameObject);
        }      
        
    }

    private void Start()
    {
        int n = tune.Length;
        string musicEventName = tune[n];        
        musicEvent = FMODUnity.RuntimeManager.CreateInstance(musicEventName);
        musicEvent.start();
    } 
    
}
