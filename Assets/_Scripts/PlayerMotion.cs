using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;
public class PlayerMotion : MonoBehaviour
{
    public Transform cam;
    public CinemachineFreeLook cinemachineFreeLook;
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public GameObject targetCam;
    public Transform targetPlayer;
    public Transform follow;
    public float speed;
    public float speedRotation = 10;
    public float groundDistanceUp, groundDistance;
    public float gravity = 9.8f;
    public float gravityMultiplier = 1;
    public float jumpPower = 35;
    public float rollPower, dodgePower, rollMultiplier;
    public float rotationSpeedCamX, rotationSpeedCamY;
    public float maxSlopeAngle = 40;
    public float playerHeight = 0.2f;
    public bool onGround, isJump;
    public bool stop;
    public bool focus;
    public bool isRoll;
    public bool interacting;
    public ItemCollision chest;
    public Door door;
    public LayerMask groundLayer;
    private PlayerCombat playerCombat;
    public PlayerCombat GetPlayerCombat => playerCombat;
    private float slopeAngle;
    private Rigidbody rb;
    private Animator anim;
    private Vector2 _move, m_look;
    private Vector3 move;
    private RaycastHit slopeHit;
    private zTarget zTarget;
    private Sequence sequence;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        zTarget = GetComponent<zTarget>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + (Vector3.up * groundDistanceUp), groundDistance);
    }

    private void FixedUpdate()
    {
        bool onSlope = OnSlope();

        rb.useGravity = !onSlope;
        groundDistanceUp = (onSlope) ? -.2f : .2f;

        onGround = Physics.CheckSphere(transform.position + (Vector3.up * groundDistanceUp), groundDistance, groundLayer);

        if (!onGround && !onSlope) rb.AddForce(-gravity * gravityMultiplier * Vector3.up, ForceMode.Acceleration);

        if (isJump && onGround)
        {
            isJump = false;
            anim.SetBool("OnAir", false);
            rb.velocity = Vector3.zero;
            playerCombat.isAttacking = false;
        }
        else if (!isJump && !onGround)
        {
            anim.SetBool("OnAir", true);
            isJump = true;
            Stopping();
            anim.SetTrigger("Fall");
        }

        // FOCUS
        if (focus && !interacting) UpdateFocus();

        // ATTACK
        if (playerCombat.isAttacking || isJump) return;

        // STOPPED
        if (stop) return;

        if (!focus)
        {
            // NORMAL MOVEMENT
            if (_move.x != 0 || _move.y != 0)
            {
                // Movement
                move = cam.forward * _move.y;
                move += cam.right * _move.x;
                move.Normalize();
                move.y = 0;
                // Movement assignment
                rb.velocity = (onSlope) ? GetSlopeMoveDirection() * speed : move * speed;

                // Rotation
                Vector3 dir = cam.forward * _move.y;
                dir += cam.right * _move.x;
                dir.y = 0;

                Quaternion targetR = Quaternion.LookRotation(dir);
                Quaternion playerR = Quaternion.Slerp(transform.rotation, targetR, speedRotation * Time.fixedDeltaTime);
                transform.rotation = playerR;
            }
        }
        else
        {
            // MOVEMENT ON FOCUS
            move = cam.forward * _move.y;
            move += cam.right * _move.x;
            move.Normalize();
            move.y = 0;

            rb.velocity = (onSlope) ? GetSlopeMoveDirection() * speed : move * speed;
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight) && onGround)
        {
            slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return slopeAngle <= maxSlopeAngle && slopeAngle != 0;
        }
        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(move, slopeHit.normal).normalized;
    }

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();

        if (stop || interacting || playerCombat.isAttacking) return;

        anim.SetBool("Move", (_move.x == 0 && _move.y == 0) ? false : true);
        anim.SetFloat("Moving", (_move.x == 0 && _move.y == 0) ? 0 : 1);

        if (_move.x == 0 && _move.y == 0) rb.velocity = Vector3.zero;

        anim.SetFloat("MoveX", _move.x);
        anim.SetFloat("MoveY", _move.y);
    }
    public void OnJump()
    {
        if (stop || !Attack()) return;

        playerCombat.Reset();

        Stopping();

        if (focus)
        {
            if (_move.x != 0 || _move.y != 0)
            {
                if (MathF.Abs(_move.x) > Mathf.Abs(_move.y)) _move.y = 0;
                else if (MathF.Abs(_move.x) < Mathf.Abs(_move.y)) _move.x = 0;
                else if (MathF.Abs(_move.x) == Mathf.Abs(_move.y)) _move.y = 0;

                if (_move.x != 0) _move.x = (_move.x < 0) ? -1f : 1f;
                if (_move.y != 0) _move.y = (_move.y < 0) ? -1f : 1f;

                anim.SetFloat("MoveX", _move.x);
                anim.SetFloat("MoveY", _move.y);

                Vector3 move = cam.forward * _move.y;
                move += cam.right * _move.x;
                move.Normalize();
                move.y = 0;

                if (_move.x != 0)
                {
                    rb.AddForce(move * dodgePower * rollMultiplier, ForceMode.Impulse);
                }
                else
                {
                    rb.AddForce(move * rollPower * rollMultiplier, ForceMode.Impulse);
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(cam.forward * rollPower, ForceMode.Impulse);
            }

            anim.SetTrigger("Jumping");
            isRoll = true;

            sequence = DOTween.Sequence();
            sequence.AppendInterval(.5f).OnComplete(() =>
            {
                if (isRoll) StopEnd();
            });
        }
        else
        {
            isJump = true;
            Vector2 moveDir = _move;
            anim.SetTrigger("Jumping");

            if (moveDir != Vector2.zero)
            {
                // Direction
                Vector3 dir = cam.forward * moveDir.y;
                dir += cam.right * moveDir.x;
                dir.Normalize();
                dir.y = 0;

                // Rotation
                Quaternion targetR = Quaternion.LookRotation(dir);
                transform.rotation = targetR;

                // Jump
                rb.AddForce((transform.forward + Vector3.up) * jumpPower, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }

            anim.SetBool("OnAir", true);
        }
    }
    public void OnUse()
    {
        if (!Attack()) return;

        if (chest)
        {
            chest.Open();
            return;
        }

        if (door)
        {
            door.Open();
        }
    }
    public void OnCam(InputValue value)
    {
        if (interacting) return;

        m_look = value.Get<Vector2>();
        cinemachineFreeLook.m_XAxis.Value += m_look.x * rotationSpeedCamX;
        cinemachineFreeLook.m_YAxis.Value += m_look.y * rotationSpeedCamY * Time.fixedDeltaTime;
    }

    public void OnFocus(InputValue value)
    {
        focus = value.isPressed;
        if (stop || isJump) return;

        IsFocus();
    }

    public void OnChangeTargetL()
    {
        if (targetPlayer == null) return;
        TargetActive(false);
        targetPlayer = zTarget.NextToLeft();
        TargetActive(true);
        UpdateFocus();
    }

    public void OnChangeTargetR()
    {
        if (targetPlayer == null) return;
        TargetActive(false);
        targetPlayer = zTarget.NextToRight();
        TargetActive(true);
        UpdateFocus();
    }

    public void SelectTarget(Transform target)
    {
        if (targetPlayer) TargetActive(false);
        targetPlayer = null;

        cinemachineVirtualCamera.Priority = 10;
        cinemachineFreeLook.Priority = 8;

        targetCam.transform.LookAt(target);

        follow.position = targetCam.transform.position;
        follow.rotation = targetCam.transform.rotation;
        transform.localEulerAngles = new Vector3(0, follow.localEulerAngles.y, 0);
    }

    public void NoTarget()
    {
        targetPlayer = null;
        UpdateFocus();
        cinemachineVirtualCamera.Priority = 8;
        cinemachineFreeLook.Priority = 10;
        IsFocus();
    }

    public void IsFocus()
    {
        if (focus)
        {
            if (targetPlayer == null) targetPlayer = zTarget.FirstTarget();

            if (targetPlayer == null)
            {
                focus = false;
                return;
            }

            TargetActive(true);
            cinemachineVirtualCamera.Priority = 10;
            cinemachineFreeLook.Priority = 8;
            anim.SetBool("IsFocus", true);
            anim.SetTrigger("Focus");

            if (playerCombat.actualWeapon != null)
            {
                if (playerCombat.actualWeapon.type == WeaponType.shield) playerCombat.OnShieldUse(true);
            }
        }
        else
        {
            if (targetPlayer != null) TargetActive(false);

            zTarget.target = null;
            targetPlayer = null;

            cinemachineVirtualCamera.Priority = 8;
            cinemachineFreeLook.Priority = 10;
            anim.SetBool("IsFocus", false);
            anim.SetTrigger("SwitchWeapon");

            playerCombat.ShieldBlock();
        }
    }

    public void UpdateFocus()
    {
        targetCam.transform.LookAt(targetPlayer);
        follow.position = targetCam.transform.position;
        follow.rotation = targetCam.transform.rotation;

        transform.localEulerAngles = new Vector3(0, follow.localEulerAngles.y, 0);
    }

    void TargetActive(bool b)
    {
        TargetDamage targetDamage = targetPlayer.GetComponent<TargetDamage>();
        if (targetDamage) targetDamage.targetPoint.SetActive(b);
    }

    public void FallEnd()
    {
        StopEnd();
    }

    public void Stopping()
    {
        isRoll = false;
        if (onGround) rb.velocity = Vector3.zero;
        stop = true;
        anim.SetFloat("MoveX", 0);
        anim.SetFloat("MoveY", 0);
        anim.SetFloat("Moving", 0);
        anim.SetBool("Move", false);
    }

    public void StopEnd()
    {
        anim.SetBool("Move", (_move.x == 0 && _move.y == 0) ? false : true);
        anim.SetFloat("Moving", (_move.x == 0 && _move.y == 0) ? 0 : 1);
        anim.SetFloat("MoveX", _move.x);
        anim.SetFloat("MoveY", _move.y);
        isRoll = false;
        rb.velocity = Vector3.zero;
        stop = false;
        IsFocus();
    }

    public void RollStop()
    {
        rb.velocity = Vector3.zero;
    }

    public bool Attack()
    {
        return !isJump && !stop && onGround;
    }
}
