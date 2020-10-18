using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui.Panels
{
    public class GameOverUi : MonoBehaviour, UiPanel
    {   
        public void Initialize()
        { 
            _quitButton.onClick.AddListener(GoToMainMenu);
        }

        private void GoToMainMenu()
        {
            FindObjectOfType<UiManager>().GoBackToMainMenu();
            
            // this is a hack, to reset any active tasks in the scene for next game
            var tasks = FindObjectsOfType<Task>();
            foreach (var task in tasks)
            {
                task.DisableTask();
            }
        }

        private void PlayAgain()
        {
            // TODO
        }

        public void SetGameOverMessage(string message)
        {
            _gameOverText.text = message;
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }
        
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private TMP_Text _gameOverText;
    }
}