using Scriptable;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance{get; private set;}
    public CameraMovements cameraMovements;
    public LinkData _data;
    public Renderer meshRenderer;
    public Transform groundCheck;
    
    public Rigidbody rigidbody{get; private set;}
    public PlayerContoller playerContoller{get; private set;}
    public PlayerJump playerJump{get; private set;}
    void Awake()
    { 
        instance = this;
        Init();
    }

    public void Init()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerContoller = GetComponent<PlayerContoller>();
        playerJump = GetComponent<PlayerJump>();
    }
}
