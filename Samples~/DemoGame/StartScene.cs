using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Xasu;
using Xasu.HighLevel;
using Simva;

namespace Simva
{
    public class StartScene : MonoBehaviour
    {
        public CanvasGroup buttons;
        public Button startBtn;

        public async void Start()
        {
            await Task.Yield();
            buttons.interactable = false;
            startBtn.onClick.AddListener(StartGame);
            buttons.interactable = true;
        }

        public void StartGame()
        {
            startBtn.interactable = false;
            StartCoroutine(SimvaPlugin.Instance.ManualStart());
        }
    }
}