using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance; //{ get; set; }

    public string[] tunes;

    private int currentTrack;

    public FMOD.Studio.EventInstance musicEvent;

    //public string musicEventName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }    
        else
        {
            Destroy(gameObject);
        }      
    }

    private void Start()
    {
        currentTrack = 2;
        PlayTrack(currentTrack);
    }

    public void PlayTrack(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < tunes.Length +1)
        {
            musicEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicEvent.release();

            currentTrack = trackIndex;
            string musicEventName = tunes[currentTrack - 1];
            musicEvent = FMODUnity.RuntimeManager.CreateInstance(musicEventName);
            musicEvent.start();
        }
        else
        {
            Debug.LogWarning("INDEX OUT OF RANGE DUMMY");
        }        
    }
}
