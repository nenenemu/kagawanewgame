using UnityEngine;

public class EggCollision : MonoBehaviour
{
    [Header("吹っ飛ばし")]
    public float knockbackPower = 100f;
    public float minimumSpeed = 0.1f;

    [Header("連続ヒット防止")]
    public float hitCooldown = 0.25f;

    private Rigidbody myRigidbody;

    private GameObject lastHitObject;
    private float lastHitTime = -999f;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();

        if (myRigidbody == null)
        {
            Debug.LogError(
                gameObject.name +
                " : EggCollisionにはRigidbodyが必要です。"
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (myRigidbody == null)
            return;

        // 相手のRigidbody
        Rigidbody targetRigidbody = collision.rigidbody;

        if (targetRigidbody == null)
            return;

        if (targetRigidbody == myRigidbody)
            return;

        // 相手が卵か確認
        EggPrefab targetEgg =
            targetRigidbody.GetComponent<EggPrefab>();

        if (targetEgg == null)
            return;

        // 同じ相手への連続ヒット防止
        if (lastHitObject == targetRigidbody.gameObject &&
            Time.time < lastHitTime + hitCooldown)
        {
            return;
        }

        // 自分の現在速度
        Vector3 velocity = myRigidbody.linearVelocity;

        // Y方向は無視
        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        // 速度
        float speed = horizontalVelocity.magnitude;

        if (speed < minimumSpeed)
            return;

        // 自分が進んでいる方向
        Vector3 direction =
            horizontalVelocity.normalized;

        // =========================
        // 吹っ飛ばし量
        // =========================

        float force =
            speed * knockbackPower;

        // 相手を吹っ飛ばす
        targetRigidbody.AddForce(
            direction * force,
            ForceMode.Impulse
        );

        // 記録
        lastHitObject =
            targetRigidbody.gameObject;

        lastHitTime =
            Time.time;

        Debug.Log(
            gameObject.name +
            " → " +
            targetRigidbody.gameObject.name +
            " | 速度 : " +
            speed.ToString("F2") +
            " | 吹っ飛ばし : " +
            force.ToString("F2")
        );
    }
}