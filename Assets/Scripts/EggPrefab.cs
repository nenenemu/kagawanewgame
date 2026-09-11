using UnityEngine;

public class EggPrefab : MonoBehaviour
{
    [Header("この卵のデータ")]
    public EggData eggData;

    [Header("現在HP")]
    public float currentHP;

    public void Initialize(EggData data)
    {
        eggData = data;

        if (eggData == null)
        {
            Debug.LogError(
                gameObject.name +
                " に EggData が設定されていません。"
            );

            return;
        }

        currentHP =
            eggData.maxHP;

        Rigidbody rb =
            GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.mass =
                eggData.mass;

            // 重心を少し下げる
            rb.centerOfMass =
                new Vector3(
                    0f,
                    -0.25f,
                    0f
                );
        }
    }
}