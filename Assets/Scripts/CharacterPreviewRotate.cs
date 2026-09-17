using UnityEngine;

public class CharacterPreviewRotate : MonoBehaviour
{
    [Header("‰ñ“]‘¬“x")]
    public float rotateSpeed = 30f;

    private void Update()
    {
        transform.Rotate(
            0f,
            rotateSpeed * Time.deltaTime,
            0f
        );
    }
}