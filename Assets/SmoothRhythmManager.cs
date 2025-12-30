using UnityEngine;

[RequireComponent(typeof(RhythmManager))]
public class SmoothRhythmManager : MonoBehaviour
{
  private float bpm;

  [Header("Sync Settings")]
  [Range(0.1f, 100f)]
  public float syncSpeed = 2.0f; // どのくらいの速さで正しい時間に寄せるか
  public float maxDrift = 0.2f;  // 許容できる最大のズレ（これ以上なら強制ワープ）

  public double CurrentBeat { get; private set; }
  public double SecPerBeat { get; private set; }
  public double SmoothedTime => _smoothedTime;

  private double _startTime;
  private double _smoothedTime;

  //private float startTimeTime;
  void Start()
  {
    var rhythmManager = GetComponent<RhythmManager>();
    bpm = rhythmManager.bpm;

    SecPerBeat = 60.0 / bpm;
    // 開始時間を少し未来に設定して安定させる
    _startTime = AudioSettings.dspTime;
    _smoothedTime = _startTime;
  }

  void Update()
  {
    // 1. dspTimeベースの「真実の時間」を計算
    double dspNow = AudioSettings.dspTime;

    // 2. 基本は unscaledDeltaTime で時間を進める
    _smoothedTime += Time.unscaledDeltaTime;

    // 3. 「少しずつ寄せる」補正ロジック
    double drift = dspNow - _smoothedTime;

    //Debug.Log($"Drift: {drift:F4} sec");

    if (System.Math.Abs(drift) > maxDrift)
    {
      // ズレが大きすぎる場合は、音楽が飛んだと判断して強制同期（ワープ）
      _smoothedTime = dspNow;

      Debug.Log(">> Forced Sync <<");
    }
    else
    {
      // ズレを syncSpeed に応じて徐々に解消する
      // 現在の時間に、ズレの数%を加算して少しずつ dspNow に近づける
      _smoothedTime += drift * Time.unscaledDeltaTime * syncSpeed;
    }

    // 4. 補正後の時間から現在の拍を計算
    CurrentBeat = _smoothedTime / SecPerBeat;
  }
}