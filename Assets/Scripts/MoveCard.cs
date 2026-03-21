using UnityEngine;
using DG.Tweening;

public class SpriteMover : MonoBehaviour
{
    void Start()
    {
        // 僅針對 X 軸移動到 10 的位置
        transform.DOMoveX(10f, 2f).SetEase(Ease.Linear);

        // 或是讓物件跳動到某個位置 (2D 常用效果)
        // 參數：目標位置, 跳躍力道, 跳躍次數, 持續時間
        transform.DOJump(new Vector3(5, 0, 0), 2f, 3, 2f);
    }
}