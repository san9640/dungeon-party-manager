using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.UI.Event;
using Dpm.User;
using TMPro;
using UnityEngine;

namespace Dpm.Stage.UI
{
    public class GameWinUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI totalUnitsText;
        

        public void Show(int totalUnits)
        {
            totalUnitsText.text = totalUnits.ToString();

            // TODO : PANEL FADE IN
            gameObject.SetActive(true);
        }

        public void OnExitButtonPressed()
        {
            CoreService.Event.Publish(ExitStageEvent.Instance);
        }

        public void OnRetryButtonPressed()
        {
            CoreService.Event.Publish(RetryButtonPressedEvent.Instance);
        }
    }
}