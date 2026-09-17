using UnityEngine;

public class EggCollision : MonoBehaviour
{
    [Header("êÅÇ¡îÚÇŒÇµ")]
    public float knockbackPower = 100f;
    public float minimumSpeed = 0.1f;

    [Header("òAë±ÉqÉbÉgñhé~")]
    public float hitCooldown = 0.25f;

    private Rigidbody myRigidbody;

    private GameObject lastHitObject;
    private float lastHitTime = -999f;

    private MatchScoreManager scoreManager;


    private void Awake()
    {
        myRigidbody =
            GetComponent<Rigidbody>();

        scoreManager =
            FindFirstObjectByType<MatchScoreManager>();


        if (myRigidbody == null)
        {
            Debug.LogError(
                gameObject.name +
                " : EggCollisionÇ…ÇÕRigidbodyÇ™ïKóvÇ≈Ç∑ÅB"
            );
        }
    }


    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (myRigidbody == null)
            return;


        Rigidbody targetRigidbody =
            collision.rigidbody;


        if (targetRigidbody == null)
            return;


        if (targetRigidbody == myRigidbody)
            return;


        EggPrefab targetEgg =
            targetRigidbody.GetComponent<EggPrefab>();


        if (targetEgg == null)
            return;


        // òAë±ÉqÉbÉgñhé~
        if (
            lastHitObject ==
            targetRigidbody.gameObject
            &&
            Time.time <
            lastHitTime + hitCooldown
        )
        {
            return;
        }


        Vector3 velocity =
            myRigidbody.linearVelocity;


        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );


        float speed =
            horizontalVelocity.magnitude;


        if (speed < minimumSpeed)
            return;


        Vector3 direction =
            horizontalVelocity.normalized;


        float force =
            speed * knockbackPower;


        targetRigidbody.AddForce(
            direction * force,
            ForceMode.Impulse
        );


        // =====================================
        // ÅöíNÇ™íNÇçUåÇÇµÇΩÇ©ãLò^
        // =====================================

        if (scoreManager != null)
        {
            scoreManager.RegisterHit(
                gameObject,
                targetRigidbody.gameObject
            );
        }


        lastHitObject =
            targetRigidbody.gameObject;

        lastHitTime =
            Time.time;


        Debug.Log(
            gameObject.name +
            " Å® " +
            targetRigidbody.gameObject.name +
            " | ë¨ìx : " +
            speed.ToString("F2") +
            " | êÅÇ¡îÚÇŒÇµ : " +
            force.ToString("F2")
        );
    }
}