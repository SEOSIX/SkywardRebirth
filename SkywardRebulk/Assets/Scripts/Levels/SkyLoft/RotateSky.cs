using UnityEngine;

public class RotateSky : MonoBehaviour
{
     [SerializeField]private float rotateSpeed;

    // Update is called once per frame
    void Update()
    {
        Rotation(rotateSpeed);
    }

    public void Rotation(float speed)
    {
        Vector3 rot = transform.localEulerAngles;
        rot.y += speed * Time.deltaTime;
        transform.localEulerAngles = rot;
    }
}