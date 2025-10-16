using System;
using UnityEngine;

public class PointScoredFeedback : MonoBehaviour
{
    [Serializable]
    struct PointScoredFeedbackEffect
    {
        public ShotType shotType;
        public ParticleSystem particle;
        public AudioClip audioClip;
    }

    [SerializeField] private PointScoredFeedbackEffect[] feedbackEffects;
    [SerializeField] private AudioSource audioSource;

    private BasketballGameManager _gameManager;

    private void OnEnable()
    {
        _gameManager = BasketballGameManager.Instance;

        _gameManager.onPointScored.AddListener(OnPointScored);
    }

    private void OnDisable()
    {
        if (_gameManager != null)
            _gameManager.onPointScored.RemoveListener(OnPointScored);
    }

    private void OnPointScored(PointScoredArgs pointScoredArgs)
    {
        foreach (var effect in feedbackEffects)
        {
            if(effect.shotType == pointScoredArgs.shotData.shotType)
            {
                if(audioSource != null && effect.audioClip != null)
                {
                    audioSource.clip = effect.audioClip;
                    audioSource.Play();
                }

                if(effect.particle != null && !effect.particle.isPlaying)
                {
                    effect.particle.Play();
                }
            }
        }
    }
}
