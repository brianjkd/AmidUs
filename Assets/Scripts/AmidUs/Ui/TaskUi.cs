using System;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui
{
    public class TaskUi : MonoBehaviour, UiPanel
    {
        public GameObject Panel;
        public Button Cancel;
        private Toggle[] _toggles;
        
        private Action _onTaskCompleteAction;
        
        public void Initialize()
        {
            Cancel.onClick.AddListener(OnCancelButton);
            
            _toggles = GetComponentsInChildren<Toggle>();
            foreach (var toggle in _toggles)
            {
                // toggle.onValueChanged.AddListener(ToggleValueChanged);
                toggle.onValueChanged.AddListener(delegate {
                    ToggleValueChanged(toggle);
                });
            }
        }

        void OnCancelButton()
        {
            var uiManager = FindObjectOfType<UiManager>();
            
            // this check prevents going to Gameplay UI while Voting or GameOver screen is visible
            if (uiManager.CurrentDisplayed == PanelType.Task)
            {
                uiManager.Show(PanelType.Gameplay);
            }
        }

        void ToggleValueChanged(Toggle toggle)
        {
            if (AllLightsAreOn())
            {
                Debug.Log("All tasks completed");
                _onTaskCompleteAction.Invoke();
                OnCancelButton();
            }
        }

        private void ToggleAllLightsOff()
        {
            foreach (var toggle in _toggles)
            {
                toggle.isOn = false;
            }
        }

        private bool AllLightsAreOn()
        {
            foreach (var toggle in _toggles)
            {
                if (!toggle.isOn)
                {
                    return false;
                }
            }
            // all toggles are on, task is complete
            return true;
        }

        public void SetOnTaskCompleteAction(Action onTaskCompleteAction)
        {
            _onTaskCompleteAction = onTaskCompleteAction;
        }

        public void Hide()
        {
            Panel.SetActive(false);
        }

        public void Show()
        {
            ToggleAllLightsOff();
            Panel.SetActive(true);
        }
    }
}