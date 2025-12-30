using UnityEngine;

public class NoteController : MonoBehaviour
{
  public double targetBeat;      // このノーツを叩くべき拍
  private float scrollSpeed = 5f; // スクロール速度（1拍で進む距離）

  private RhythmManager _manager;
  private Vector3 _startPos;
  private Vector3 _targetPos;

  public void Initialize(RhythmManager manager, double beat, Vector3 targetPosition, double offset, double noteScrollSpeed)
  {
    _manager = manager;
    targetBeat = beat;
    _targetPos = targetPosition;
    scrollSpeed = (float)noteScrollSpeed;

    var audioSource = GetComponent<AudioSource>();

    //audioSource.PlayScheduled(
    //  manager.startTime + manager.musicStartDelay + offset +
    //  (targetBeat * (60.0 / _manager.bpm))
    //);

    // ターゲット位置から逆算して、今の位置（初期位置）を決める
    // 例：左から右に流れる場合、targetBeatの時点にtargetPosに届くように配置
    UpdatePosition();
  }

  void Update()
  {
    if (_manager == null) return;

    UpdatePosition();

    if (_manager.CurrentBeat > targetBeat + 0.5)
    {
      //miss処理
      var rhythmJudge = _manager.GetComponent<RhythmJudge>();
      rhythmJudge.JudgeMiss(targetBeat);
    }

    // 判定ラインを通り過ぎて一定時間経ったら消す
    if (_manager.CurrentBeat > targetBeat + 2)
    {
      Destroy(gameObject);
    }
  }

  void UpdatePosition()
  {
    // 現在の拍と目標の拍の差分を計算
    // (目標拍 - 現在拍) * 速度 = ターゲット位置からのオフセット
    float beatOffset = (float)(targetBeat - ((_manager.CurrentDspTime + _manager.tapOffset) / _manager._secPerBeat));

    // ターゲット位置（判定ライン）を基準に、X座標をずらす
    transform.position = _targetPos + new Vector3(beatOffset * scrollSpeed, 0, 0);
  }
}