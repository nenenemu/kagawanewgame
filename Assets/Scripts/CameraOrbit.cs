using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("‰ñ“]‘¬“x")]
    public float rotateSpeed = 5f;

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
}