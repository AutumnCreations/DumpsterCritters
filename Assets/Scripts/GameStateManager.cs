using UnityEngine;
using Sirenix.OdinInspector;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Dialogue,
        Paused,
    }
    
    [BoxGroup("State Control")]
    [ReadOnly]
    public GameState currentState;

    [BoxGroup("Game Configuration")]
    [Range(0, 100)]
    public int masterVolume;

    [BoxGroup("Game Configuration")]
    [Range(0, 100)]
    public int musicVolume;

    [BoxGroup("Game Configuration")]
    [Range(0, 100)]
    public int sfxVolume;

    public GameState CurrentState { get; private set; }
    public static GameStateManager Instance { get; private set; }
    public delegate void OnGameStateChange(GameState newState);
    public event OnGameStateChange onGameStateChange;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        onGameStateChange?.Invoke(newState);
    }

    public void SetMasterVolume(int newVolume)
    {
        masterVolume = newVolume;
    }

    public void SetMusicVolume(int newVolume)
    {
        musicVolume = newVolume;
    }

    public void SetSFXVolume(int newVolume)
    {
        sfxVolume = newVolume;
    }

}
