using UnityEngine;

public class GoodMetronome : MonoBehaviour
{

  [SerializeField] AudioSource _ring;

  double _bpm = 140d;
  double _metronomeStartDspTime;
  double _buffer = 2 / 60d;

  private float previousBeatTime = 0f;

  void Start()
  {
    _metronomeStartDspTime = AudioSettings.dspTime;
  }

  void FixedUpdate()
  {
    var nxtRng = NextRingTime();

    if (nxtRng < AudioSettings.dspTime + _buffer)
    {
      _ring.PlayScheduled(nxtRng);
      Debug.Log($"Time since last beat: {Time.time - previousBeatTime} seconds");
      previousBeatTime = Time.time;
    }
  }

  double NextRingTime()
  {
    var beatInterval = 60d / _bpm;
    var elapsedDspTime = AudioSettings.dspTime - _metronomeStartDspTime;
    var beats = System.Math.Floor(elapsedDspTime / beatInterval);

    return _metronomeStartDspTime + (beats + 1d) * beatInterval;
  }
}

