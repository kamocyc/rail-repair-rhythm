using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSongData", menuName = "RhythmGame/SongData")]
public class SongData : ScriptableObject
{
  public string songName;
  public AudioClip musicClip;
  public float bpm;
  public double musicStartDelay;

  public List<int> beats = new List<int>();
}
