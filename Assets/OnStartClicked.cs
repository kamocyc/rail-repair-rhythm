using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartClicked : MonoBehaviour
{
  private void Start()
  {
    // add listener to button
    UnityEngine.UI.Button btn = GetComponent<UnityEngine.UI.Button>();
    btn.onClick.AddListener(OnClick);
  }

  void OnClick()
  {
    UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
  }
}
