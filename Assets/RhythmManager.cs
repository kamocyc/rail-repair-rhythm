using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(SmoothRhythmManager))]
[RequireComponent(typeof(RhythmJudge))]
[RequireComponent(typeof(AudioSource))]
public class RhythmManager : MonoBehaviour
{
  public float bpm = 120f;
  public Image targetVisual; // 点滅させるUI
  public Color flashColor = Color.white;
  public Color normalColor = Color.gray;

  public double _secPerBeat;    // 1拍の長さ(秒)
  private double _nextBeatTime;  // 次の拍が来るdspTime
  private int _beatCount = 0;    // 拍数カウント

  public GameObject notePrefab;
  public Transform spawnPoint;
  public Transform judgeLine; // 判定ラインの位置

  public double startTime;

  private AudioSource audioSource;
  public double startDelay = 1.0; // 曲開始までの遅延時間（秒）
  public double musicStartDelay = 0.2f;  // 音楽先頭の無音部分の長さ（秒）

  public double tapOffset = 0;
  public double noteScrollSpeed = 5.0;  // ノーツのスクロール速度（1拍で進む距離）

  // 現在の曲の進行状況（何拍目か）を小数点以下まで取得
  public double CurrentDspTime => AudioSettings.dspTime - startTime - musicStartDelay;
  public double CurrentBeat => CurrentDspTime / _secPerBeat;

  public SongData songData;

  public bool isPlaying = false;

  public GameObject trainGo;
  public GameObject resultPanel;

  IEnumerator PlayAfer(double delay)
  {
    yield return new WaitForSeconds((float)delay);
    audioSource.UnPause();
  } 

  void Start()
  {
    // Load song data
    audioSource = GetComponent<AudioSource>();
    audioSource.clip = songData.musicClip;
    bpm = songData.bpm;
    musicStartDelay = songData.musicStartDelay;

    audioSource = GetComponent<AudioSource>();
    _secPerBeat = 60.0 / bpm;

    audioSource.Play();
    audioSource.Pause();
    StartCoroutine(PlayAfer(startDelay));
    //audioSource.PlayScheduled(startTime);

    startTime = AudioSettings.dspTime + startDelay;
    _nextBeatTime = 0;

    var rhythmJudge = GetComponent<RhythmJudge>();
    rhythmJudge.tapOffset = tapOffset;

    Application.targetFrameRate = 60;

    var beats = new List<int>(songData.beats);
    //var beats = new List<int> { 1 };
    AppendRandomNotes(beats);
    foreach (var beat in beats)
    {
      SpawnNote(beat);
    }

    RegisterSongEnd(beats[beats.Count - 1] + 3);

    isPlaying = true;
  }

  void AppendRandomNotes(List<int> beats)
  {
    for (int i = 0; i < 50; i++)
    {
      var lastBeat = beats[beats.Count - 1];
      var randomOffset = UnityEngine.Random.Range(1, 3);
      beats.Add(lastBeat + randomOffset);
    }
  }

  void SpawnNote(double beat)
  {
    GameObject go = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
    go.GetComponent<NoteController>().Initialize(this, beat, judgeLine.position, 0, noteScrollSpeed);

    var rhythmJudge = GetComponent<RhythmJudge>();
    rhythmJudge.Register(beat, go);
  }

  private int endBeat;
  void RegisterSongEnd(double endBeat)
  {
    this.endBeat = (int)endBeat;
  }

  IEnumerator SceneResetAfterDelay(float delay)
  {
    yield return new WaitForSeconds(delay);
    var rj = GetComponent<RhythmJudge>();
    // 結果パネルを表示
    resultPanel.SetActive(true);
    var resultText = resultPanel.transform.Find("ResultText").GetComponent<Text>();
    resultText.text = $"Perfect: {rj.perfectHits}\nOK: {rj.okHits}\nMiss: {rj.missHits}\n\n" +
      $"Max Combo: {rj.maxCombo}\nScore: {rj.score}";
  }

  void Update()
  {
    if (!isPlaying) return;

    //Debug.Log("CurrentBeat: " + CurrentBeat);
    // 現在のDSP時刻が「次の拍」の予定時刻を超えたか判定
    // DSP時間がフレームによってブレブレになるので、適切に補正を入れたい、、がそれをすると全体的にずれそう
    //var r = GetComponent<SmoothRhythmManager>();

    var currentDspTime = CurrentDspTime;
    if (currentDspTime >= _nextBeatTime)
    {
      OnBeat();
      _nextBeatTime += _secPerBeat; // 次の拍の時間を設定
      _beatCount++;

      if (_beatCount >= endBeat)
      {
        isPlaying = false;
        audioSource.Stop();

        var rj = GetComponent<RhythmJudge>();
        var okRatio = (float)rj.successfulHits / songData.beats.Count;

        var character = GameObject.Find("CharaImage");
        // 1秒かけて上に移動
        character.transform.DOMoveY(character.transform.position.y + 3f, 1f)
          .SetEase(DG.Tweening.Ease.OutQuad)
          .OnComplete(() =>
          {
            rj.ResetTexts();

            // 電車を走らせる処理
            trainGo.GetComponent<TrainMove>().okRatio = okRatio;
            trainGo.SetActive(true);

            StartCoroutine(SceneResetAfterDelay(3));
          });
      }
    }

    // 視覚的なフィードバック（徐々に元の色に戻る）
    targetVisual.color = Color.Lerp(targetVisual.color, normalColor, Time.deltaTime * 10f);
  }

  public float beatDiffMin = float.MaxValue;
  public float beatDiffMax = float.MinValue;
  private float previousBeatTime = 0f;

  int count = 0;
  double offset = 0.0;
  double totalOffset = 0.0;

  void OnBeat()
  {
    // 拍の瞬間の処理
    targetVisual.color = flashColor;
    //Debug.Log($"Beat: {_beatCount}");

    var j = GetComponent<RhythmJudge>();
    //var diff3 = j.EvaluateHit(true);
    //totalOffset += diff3;
    count++;
    if (count == 3)
    {
      offset = totalOffset / 3.0;
    }

    if (previousBeatTime != 0f)
    {
      var diff = Time.time - previousBeatTime;
      if (diff < beatDiffMin)
      {
        beatDiffMin = diff;
      }
      if (diff > beatDiffMax)
      {
        beatDiffMax = diff;
      }
      //Debug.Log($"Time since last beat: {diff} seconds");
    }

    previousBeatTime = Time.time;
  }
}
