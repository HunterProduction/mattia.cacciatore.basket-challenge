using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BasketballBall : MonoBehaviour
{
    public BasketballPlayer Owner { get; set; }

    private Rigidbody _rigidbody;
    public Rigidbody Rigidbody => _rigidbody;

    private Pose _defaultLocalPose;

    private void Awake()
    {
        if(_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        var basketballLayer = LayerMask.NameToLayer("BasketBall");
        gameObject.layer = basketballLayer;

        _defaultLocalPose.position = transform.localPosition;
        _defaultLocalPose.rotation = transform.localRotation;
    }

    public void Shoot(Vector3 velocity)
    {
        transform.parent = null;
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        _rigidbody.AddTorque(-transform.right * Random.Range(0, 1f));
    }

    public void ResetBall()
    {
        _rigidbody.isKinematic = true;
        transform.parent = Owner.transform;
        transform.SetLocalPositionAndRotation(_defaultLocalPose.position, _defaultLocalPose.rotation);
    }
}
