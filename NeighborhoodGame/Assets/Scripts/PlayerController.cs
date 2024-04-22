using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Vector3 CurrentDirection { get; private set; } = Vector2.up;
    public bool IsInvincible { get; private set; } = false;
    private float _horizontalInput, _coyoteTimeCounter = 0, _jumpBufferCounter = 0, _currentJumpForce = 20f;
    private bool _isDodging, _noMovement, _isDead = false, _isGrounded = false, _performJump = false;
    private readonly float _topSpeed = 12f, _timeToTopSpeed = .2f, _degradeInertiaMultiplier = 6f, _dashMultiplier = 1.5f, _jumpBufferTime = .2f, _coyoteTime = .25f;
    [SerializeField] private Rigidbody2D _playerRB;
    [SerializeField] private Animator _spriteAnimator;
    [SerializeField] private AudioClip _playerJumpClip;

    private void Awake()
    {
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDodging || _noMovement || _isDead || TimeManager.Instance.IsPaused)
            return;

        _horizontalInput = Input.GetAxisRaw("Horizontal");
        // Coyote Time Logic
        if (_isGrounded)
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump Buffer Logic
        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }

        // Control jumping
        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            _performJump = true;
            _jumpBufferCounter = 0;
        }

        if (Input.GetButtonUp("Jump") && _playerRB.velocity.y > 0f)
        {
            _playerRB.velocity = new Vector2(_playerRB.velocity.x, _playerRB.velocity.y * .5f);
            _coyoteTimeCounter = 0;
        }

        SetSpriteAnimations(_horizontalInput, 0);
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        Vector3 targetDirection = new Vector3(_horizontalInput, 0, 0).normalized;
        if (_isDodging || _noMovement)
            targetDirection = CurrentDirection;

        Move(targetDirection);

        // Control jumping
        if (_performJump)
        {
            StartJump();
        }
    }

    private void SetSpriteAnimations(float horizontalInput, float verticalInput)
    {
        // Animator updates; does not rewrite direction on zero to avoid changing sprite direction when input is released
        /*if (_horizontalInput != 0 || _verticalInput != 0)
        {
            _spriteAnimator.SetBool("isWalking", true);
            _spriteAnimator.SetFloat("XInput", horizontalInput);
            _spriteAnimator.SetFloat("YInput", verticalInput);
        }
        else
        {
            _spriteAnimator.SetBool("isWalking", false);
        }*/
    }

    private void Move(Vector3 targetDirection)
    {
        var stopMovement = _noMovement;
        if (targetDirection.magnitude > 0)
        {
            CurrentDirection = targetDirection;
        }
        else
        {
            stopMovement = true;
        }

        if (!stopMovement)
        {
            _playerRB.drag = 0;
            Vector3 targetVelocity = targetDirection.normalized * _topSpeed;
            Vector2 diffVelocity = new Vector2(targetVelocity.x - _playerRB.velocity.x, 0);
            if (targetVelocity.x == 0)
                diffVelocity.x *= _degradeInertiaMultiplier;
            _playerRB.AddForce(diffVelocity / _timeToTopSpeed);
        }
        else if (_isGrounded)
        {
            // _playerRB.drag = _topSpeed / _timeToTopSpeed;
        }
    }

    private void StartJump()
    {
        // _spriteAnimator.SetTrigger("Jump");
        _playerRB.AddForce(Vector3.up * _currentJumpForce, ForceMode2D.Impulse);
        _performJump = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGrounding(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGrounding(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _isGrounded = false;
    }

    private void CheckGrounding(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            _isGrounded |= Vector2.Angle(normal, Vector2.up) < 90;
        }
    }
}
