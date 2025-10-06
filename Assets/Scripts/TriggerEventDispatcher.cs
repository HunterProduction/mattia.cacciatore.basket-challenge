using UnityEngine;
using UnityEngine.Events;

public class TriggerEventDispatcher : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    public UnityEvent onTriggerEnter; 

    private Collider _trigger;

    private void Awake()
    {
        _trigger = GetComponentInChildren<Collider>();
        _trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsLayerInLayerMask(other.gameObject.layer, layerMask))
        {
            Debug.Log($"[{GetType().Name}] {other.gameObject.name} entered in {this.name} trigger area");

            onTriggerEnter.Invoke();
        }
    }

    private void OnDestroy()
    {
        onTriggerEnter.RemoveAllListeners();
    }

    private bool IsLayerInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | 1 << layer);
    }
}
