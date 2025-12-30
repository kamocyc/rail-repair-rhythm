using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

//[RequireComponent(typeof(Image))]
public class BackGroundMover : MonoBehaviour
{
  private const float k_maxLength = 1f;
  private const string k_propName = "_MainTex";

  [SerializeField]
  private Vector2 m_offsetSpeed;

  private Material m_copiedMaterial;

  private RhythmManager m_rhythmManager;
  private RawImage _image;

  private void Start()
  {
    m_rhythmManager = FindObjectOfType<RhythmManager>();

    _image = GetComponent<RawImage>();
  }

  private void Update()
  {
    if (Time.timeScale == 0f)
    {
      return;
    }

    // xとyの値が0 ～ 1でリピートするようにする
    var zoom = 1;// 1150 / 10f;
    var imageX = 12f;
    var offsetX = m_rhythmManager.CurrentBeat * m_rhythmManager.noteScrollSpeed;
    // 1150 / 10
    var x = Mathf.Repeat((float)(offsetX / imageX) * zoom, k_maxLength);
    //var y = Mathf.Repeat(Time.time * m_offsetSpeed.y, k_maxLength);
    var offset = new Vector2(x, 0);
    _image.uvRect = new Rect(offset, _image.uvRect.size);
    //m_copiedMaterial.SetTextureOffset(k_propName, offset);
  }

  private void OnDestroy()
  {
    // ゲームオブジェクト破壊時にマテリアルのコピーも消しておく
    //Destroy(m_copiedMaterial);
    //m_copiedMaterial = null;
  }
}