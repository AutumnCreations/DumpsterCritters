using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class MusicStart : MonoBehaviour
{
    //[FMODUnity.EventRef]
    //public string mainMusicEvent = "event:/Music/MePlusMice";
    public FMOD.Studio.EventInstance mainMusic;

    //[FMODUnity.EventRef]
    //public string shopMusicEvent = "event:/Music/SleazyFurball";
    public FMOD.Studio.EventInstance shopMusic;

    void Awake()
    {
        mainMusic = FMODUnity.RuntimeManager.CreateInstance("event:/Music/MePlusMice");
        mainMusic.start();
    }
}
