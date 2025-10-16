using System;
using UnityEngine;

public class TriggerFeedback : MonoBehaviour
{
    /**
     * #NOTE: #MattiaCacciatore This class shares repeated code with CollisionFeedback. With some inheritance, it could be
     * avoided, but due to time reasons and for the sake of this test, it is left as is.
     */
    [SerializeField] private TriggerEventDispatcher trigger;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem particle;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    private void OnEnable()
    {
        if (trigger == null)
        {
            Debug.Log($"[{GetType().Name}] No TrivverEventDispatcher assigned", this);
            return;
        }

        /**
         * #NOTE: #MattiaCacciatore This pattern of external check could be avoided if the EventDispatcher exposes
         * a method subscribing and unsubscribing callbacks. In this way it can decided to which event subscribe internally.
         * It may be a more solid architecture, but due to time reason and for the sake of this test, it is left as is.
         */
        if (trigger.SendEventMode == SendEventMode.UnityEvent)
            trigger.onEnter.AddListener(OnEnter);
        else
            trigger.entered += OnEnter;
    }

    private void OnDisable()
    {
        if (trigger == null)
            return;

        if (trigger.SendEventMode == SendEventMode.UnityEvent)
            trigger.onEnter.RemoveListener(OnEnter);
        else
            trigger.entered -= OnEnter;
    }

    private void OnEnter(Collider collider)
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        if (audioSource != null && !particle.isPlaying)
        {
            particle.Play();
        }
    }
}
