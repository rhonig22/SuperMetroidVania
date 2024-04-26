using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrounding : MonoBehaviour
{
    public bool IsGrounded { get; private set; } = false;

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
        IsGrounded = false;
    }

    private void CheckGrounding(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            IsGrounded |= Vector2.Angle(normal, Vector2.up) < 90;
        }
    }
}
