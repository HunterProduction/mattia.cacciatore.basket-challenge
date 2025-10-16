using UnityEngine;

public class CollisionFeedback : MonoBehaviour
{
    /**
     * #NOTE: #MattiaCacciatore This class shares repeated code with TriggerFeedback. With some inheritance, it could be
     * avoided, but due to time reasons and for the sake of this test, it is left as is.
     */
    [SerializeField] private CollisionEventDispatcher collision;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private bool alignToCollisionPoint;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    private void OnEnable()
    {
        if(collision == null)
        {
            Debug.Log($"[{GetType().Name}] No CollisionEventDispatcher assigned", this);
            return;
        }

        /**
         * #NOTE: #MattiaCacciatore This pattern of external check could be avoided if the EventDispatcher exposes
         * a method subscribing and unsubscribing callbacks. In this way it can decided to which event subscribe internally.
         * It may be a more solid architecture, but due to time reason and for the sake of this test, it is left as is.
         */
        if (collision.SendEventMode == SendEventMode.UnityEvent)
            collision.onEnter.AddListener(OnEnter);
        else
            collision.entered += OnEnter;
    }

    private void OnDisable()
    {
        if (collision == null)
            return;

        if (collision.SendEventMode == SendEventMode.UnityEvent)
            collision.onEnter.RemoveListener(OnEnter);
        else
            collision.entered -= OnEnter;
    }

    private void OnEnter(Collision collision)
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        if (particle != null && !particle.isPlaying)
        {
            if(alignToCollisionPoint)
            {
                particle.transform.position = collision.contacts[0].point;
            }
            particle.Play();
        }
    }
}
