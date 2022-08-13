using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private FloatingJoystick _joystick;

    private Animator _animator;
    private Rigidbody _rigidbody;

    private Matrix4x4 _correctionMatrix;

    private bool _isMoving = false;
    private bool _isAlive = true;
    private float _moveSpeed;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();

        _correctionMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));
    }

    private void Start()
    {
        _moveSpeed = General.Instance.GameSettings.PlayerMoveSpeed;
    }

    private void FixedUpdate()
    {
        if (_joystick.Horizontal != 0 || _joystick.Vertical != 0)
        {
            if (!GetComponent<PlayerCombat>().GetAttackingStatus() && !GetComponent<PlayerMining>().GetMiningStatus())
                _animator.SetInteger("state", 1);

            _animator.SetFloat("speed", _rigidbody.velocity.magnitude);
            _isMoving = true;

            Vector3 _movement = _correctionMatrix.MultiplyPoint3x4(new Vector3(_joystick.Horizontal, 0, _joystick.Vertical)) * _moveSpeed;
            _rigidbody.velocity = new Vector3(_movement.x, _rigidbody.velocity.y, _movement.z);

            if (new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z));
        }
        else
        {
            _isMoving = false;

            if (!GetComponent<PlayerMining>().GetMiningStatus() && !GetComponent<PlayerCombat>().GetAttackingStatus())
                _animator.SetInteger("state", 0);

            _animator.SetFloat("speed", 0);

            _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
        }
    }

    public void StopAction()
    {
        _animator.SetInteger("state", 0);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = true;
        GetComponent<PlayerMining>().enabled = false;
        GetComponent<PlayerCombat>().enabled = false;
        enabled = false;
    }

    public void ResumeAction()
    {
        _rigidbody.isKinematic = false;
        GetComponent<PlayerMining>().enabled = true;
        GetComponent<PlayerCombat>().enabled = true;
        enabled = true;
    }

    public bool GetMovingStatus()
    {
        return _isMoving;
    }
}