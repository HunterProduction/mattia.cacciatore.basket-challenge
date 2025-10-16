using System;
using UnityEngine;

public class BasketballGameAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip startClip;
    [SerializeField] private AudioClip winClip;

    private BasketballGameManager _gameManager;

    private void Awake()
    {
        _gameManager = BasketballGameManager.Instance;

        _gameManager.onGameOver.AddListener(OnGameOver);
        _gameManager.onGameStarted.AddListener(OnGameStarted);
    }

    private void OnGameStarted()
    {
        audioSource.clip = startClip;
        audioSource.Play();
        _gameManager.onGameStarted.RemoveListener(OnGameStarted);
    }

    private void OnGameOver(GameOverArgs args)
    {
        if(args.matchResult == MatchResult.Win)
        {
            audioSource.Stop();
            audioSource.clip = winClip;
            audioSource.Play();
        }
        _gameManager.onGameOver.RemoveListener(OnGameOver);
    }
}
