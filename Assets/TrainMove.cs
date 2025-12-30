using DG.Tweening;
using UnityEngine;

public class TrainMove : MonoBehaviour
{
  public float moveSpeed = 5.0f;
  public float okRatio;
  //public float shakeStrength = 0.3f;
  public int shakeVibrato = 10;

  public float endX = 15;

  private void Start()
  {
    var shakeStrength = GetStrengthByRatio();

    var child = transform.GetChild(0);
    // 振動エフェクト
    var childTweener = child.transform
      .DOShakePosition(1000, shakeStrength, shakeVibrato)
      .SetLoops(int.MaxValue, LoopType.Restart);

    // 右方向への等速直線運動
    transform
      .DOMoveX(endX, Mathf.Abs(endX - transform.position.x) / moveSpeed)
      .SetEase(Ease.Linear)
      .OnComplete(() =>
      {
        Destroy(gameObject);
        childTweener.Kill();

        Debug.Log("Train destroyed");
      });

  }

  private float GetStrengthByRatio()
  {
    var badRatio = 1f - okRatio;
    if (badRatio < 0.05) return 0;
    if (badRatio < 0.1) return 0.05f;
    if (badRatio < 0.2) return 0.1f;
    if (badRatio < 0.3) return 0.2f;
    return 1f;
  }
}
