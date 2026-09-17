using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    [Header("モデルを置く場所")]
    public Transform previewParent;

    private GameObject currentModel;

    [Header("プレビューサイズ")]
    public float previewScale = 20f;

    public void ShowEgg(EggData data)
    {
        Debug.Log("★★★ ShowEgg 呼ばれた ★★★");

        ClearPreview();

        if (data == null)
        {
            Debug.LogWarning("EggDataがありません。");
            return;
        }

        if (data.previewPrefab == null)
        {
            Debug.LogWarning(
                data.name + " に Preview Prefab が設定されていません。"
            );
            return;
        }

        if (previewParent == null)
        {
            Debug.LogWarning(
                gameObject.name + " に Preview Parent が設定されていません。"
            );
            return;
        }

        currentModel = Instantiate(
            data.previewPrefab,
            previewParent
        );

        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        currentModel.transform.localScale = Vector3.one * previewScale;

        Debug.Log(
            gameObject.name +
            " : " +
            currentModel.name +
            " を生成しました"
        );
    }

    public void ClearPreview()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }
    }
}