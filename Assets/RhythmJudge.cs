using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmJudge : MonoBehaviour
{
  [Header("Settings")]
  public KeyCode hitKey = KeyCode.Space;
  public float perfectThreshold = 0.05f; // Perfectの許容誤差
  public float okThreshold = 0.15f;      // OKの許容誤差

  private RhythmManager _manager;
  public Animator _animator;

  public double tapOffset;

  public Dictionary<int, GameObject> beats;

  public GameObject perfectText;
  public GameObject okText;
  public GameObject missText;
  
  public GameObject comboText;
  public GameObject scoreText;

  public int maxCombo = 0;
  public int combo = 0;
  public int score = 0;

  public int perfectHits = 0;
  public int okHits = 0;
  public int missHits = 0;

  enum Judgment
  {
    Perfect,
    OK,
    Miss
  }

  private int currentTextCoroutine = 0;
  public int successfulHits = 0;

  public void Register(double beat, GameObject go)
  {
    if (beats == null)
    {
      beats = new Dictionary<int, GameObject>();
    }
    this.beats.Add((int)beat, go);
  }

  void Start()
  {
    _manager = GetComponent<RhythmManager>();

    comboText.GetComponent<UnityEngine.UI.Text>().text = "Combo: " + 0;
    scoreText.GetComponent<UnityEngine.UI.Text>().text = "Score: " + 0;
  }

  void Update()
  {
    if (_manager.isPlaying == false)
    {
      return;
    }

    if (Input.GetKeyDown(hitKey))
    {
      // animationを再生する
      _animator.Play("neko_knock");

      EvaluateHit();//
    }
  }

  void ShowText(Judgment judgment)
  {
    GameObject textToShow = null;
    switch (judgment)
    {
      case Judgment.Perfect:
        textToShow = perfectText;
        score += 100;
        combo++;
        successfulHits++;
        perfectHits++;
        break;
      case Judgment.OK:
        textToShow = okText;
        score += 50;
        combo++;
        successfulHits++;
        okHits++;
        break;
      case Judgment.Miss:
        textToShow = missText;
        score += 0;
        combo = 0;
        missHits++;
        break;
    }
    if (combo > maxCombo)
    {
      maxCombo = combo;
    }

    comboText.GetComponent<UnityEngine.UI.Text>().text = "Combo: " + combo;
    scoreText.GetComponent<UnityEngine.UI.Text>().text = "Score: " + score;

    // 他を非表示にする
    perfectText.SetActive(false);
    okText.SetActive(false);
    missText.SetActive(false);

    textToShow.SetActive(true);

    currentTextCoroutine++;
    StartCoroutine(HideTextAfterDelay(textToShow, 0.5f));
  }

  public void ResetTexts()
  {
    perfectText.SetActive(false);
    okText.SetActive(false);
    missText.SetActive(false);
  }

  IEnumerator HideTextAfterDelay(GameObject textObject, float delay)
  {
    yield return new WaitForSeconds(delay);
    if (currentTextCoroutine > 1)
    {
      currentTextCoroutine--;
    }
    else
    {
      textObject.SetActive(false);
    }
  }

  public void JudgeMiss(double beat)
  {
    int beatIndex = (int)beat;
    if (beats.ContainsKey(beatIndex))
    {
      beats.Remove(beatIndex);
      Debug.Log("<color=red>MISS</color> (auto)");
      ShowText(Judgment.Miss);
    }
  }

  public double EvaluateHit(bool show = true)
  {
    double elapsedTime = (_manager.GetComponent<RhythmManager>().CurrentDspTime + tapOffset);

    // 最も近い拍の時間を計算
    // 現在の経過時間を1拍の長さで割り、四捨五入することで「一番近い拍」を特定
    double beatInterval = 60.0 / _manager.bpm;
    int closestBeatIndex = (int)System.Math.Round(elapsedTime / beatInterval);
    Debug.Log("Closest Beat Index: " + closestBeatIndex + ", elapsedTime: " + System.Math.Round(elapsedTime, 3));
    if (!beats.ContainsKey(closestBeatIndex))
    {
      Debug.Log("<color=red>MISS</color> (not notes)");
      ShowText(Judgment.Miss);
      return float.MaxValue;
    }

    var note = beats[closestBeatIndex];

    double closestBeatTime = (closestBeatIndex * beatInterval);

    // 誤差を計算
    float diff = (float)System.Math.Abs(elapsedTime - closestBeatTime);

    if (show)
    {
      var nc = FindAnyObjectByType<NoteController>();
      var ac = nc.GetComponent<AudioSource>();
      if (diff <= perfectThreshold)
      {
        Debug.Log("<color=yellow>PERFECT!</color> (" + diff + "s)");
        ShowText(Judgment.Perfect);
        ac.Play();
      }
      else if (diff <= okThreshold)
      {
        Debug.Log("<color=green>OK</color> (" + diff + "s)");
        ShowText(Judgment.OK);
        ac.Play();
      }
      else
      {
        Debug.Log("<color=red>MISS</color> (" + diff + "s)");
        ShowText(Judgment.Miss);
      }

      Destroy(note);
    }

    return diff;
  }
}