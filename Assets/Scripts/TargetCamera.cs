using UnityEngine;

public class TargetCamera : MonoBehaviour
{
    [Header("プレイヤー")]
    [Tooltip("このカメラが追いかける自分の卵")]
    public Transform player;

    [Header("相手")]
    [Tooltip("カメラが見る相手の卵")]
    public Transform target;

    [Header("カメラ位置")]
    [Tooltip("プレイヤーからカメラまでの距離")]
    public float distance = 8f;

    [Tooltip("プレイヤーからのカメラの高さ")]
    public float height = 4f;

    [Header("カメラ注視")]
    [Tooltip("相手の高さ方向への注視位置")]
    public float lookHeight = 1f;

    [Header("追従")]
    public float positionSmooth = 10f;
    public float rotationSmooth = 10f;


    void LateUpdate()
    {
        // プレイヤーがいなければ何もしない
        if (player == null)
            return;

        // =====================================================
        // プレイヤー → 相手 の方向を計算
        // =====================================================

        Vector3 playerPosition = player.position;

        Vector3 direction;

        if (target != null)
        {
            direction = target.position - playerPosition;
        }
        else
        {
            // 相手がいない場合は、とりあえず前方向
            direction = player.forward;
        }

        // Y方向は無視
        direction.y = 0f;

        // プレイヤーと相手がほぼ同じ位置なら処理しない
        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();


        // =====================================================
        // カメラ位置
        // =====================================================

        // 相手とは反対側にカメラを置く
        //
        // 相手
        //   ↓
        // プレイヤー
        //   ↓
        // カメラ
        //

        Vector3 desiredPosition =
            playerPosition -
            direction * distance;

        // プレイヤーを基準に高さを決める
        desiredPosition.y =
            playerPosition.y + height;


        // =====================================================
        // カメラ移動
        // =====================================================

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                positionSmooth * Time.deltaTime
            );


        // =====================================================
        // カメラの向き
        // =====================================================

        Vector3 lookPosition;

        if (target != null)
        {
            // 相手を見る
            lookPosition =
                target.position;

            lookPosition.y += lookHeight;
        }
        else
        {
            // 相手がいなければプレイヤーを見る
            lookPosition =
                playerPosition;

            lookPosition.y += lookHeight;
        }


        Vector3 lookDirection =
            lookPosition - transform.position;

        if (lookDirection.sqrMagnitude < 0.001f)
            return;


        Quaternion desiredRotation =
            Quaternion.LookRotation(
                lookDirection
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSmooth * Time.deltaTime
            );
    }
}