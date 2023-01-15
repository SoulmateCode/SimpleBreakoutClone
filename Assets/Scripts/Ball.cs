using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float _speed = 20f;
    Rigidbody _rigidBody;
    Vector3 _velocity;
    Renderer _renderer;
    bool _gameStarted;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        Invoke("Launch", 0.0f);
    }

    void Launch()
    {
        _rigidBody.velocity = Vector3.up * _speed;

    }

    void FixedUpdate()
    {
        _rigidBody.velocity = _rigidBody.velocity.normalized * _speed;
        _velocity = _rigidBody.velocity;

        if(!_renderer.isVisible)
        {
            GameManager.Instance.Balls--;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _rigidBody.velocity = Vector3.Reflect(_velocity, collision.contacts[0].normal);
    }
}
