using System;
using UnityEngine;
using UnityEngine.UIElements;

public class RotateHelice : MonoBehaviour
{
    [SerializeField] private Vector3 rotateVector;
    [SerializeField] private float rotateSpeed;

    private Transform child;

    private void Awake()
    {
        child = GetComponentInChildren<Transform>();
    }

    void FixedUpdate()
    {
        Rotation();
    }
    
    void Rotation()
    {
        child.Rotate(rotateVector, rotateSpeed);
    }
}
