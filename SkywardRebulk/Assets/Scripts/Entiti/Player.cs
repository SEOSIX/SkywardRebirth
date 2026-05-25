using Entiti;
using Scriptable;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance{get; private set;}
    public CameraMovements cameraMovements;
    public LinkData _data;
    public Renderer meshRenderer;
    public CapsuleCollider Collider;
    
    public Rigidbody rigidbody{get; private set;}
    public PlayerContoller playerContoller{get; private set;}
    public PlayerJump playerJump{get; private set;}
    public PlayerPhysic playerPhysic{get; private set;}
    void Awake()
    { 
        instance = this;
        Init();
    }

    
    public void Init()
     
    {
        rigidbody = GetComponent<Rigidbody>();
        playerContoller = GetComponent<PlayerContoller>();
        playerPhysic = GetComponent<PlayerPhysic>();
        playerJump = GetComponent<PlayerJump>();
    }
}
