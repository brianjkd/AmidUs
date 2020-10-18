using TMPro;
using UnityEngine;

namespace AmidUs.Ui
{
    public class VoteResultsUi : MonoBehaviour, UiPanel
    {
        public void Hide()
        {
            Panel.SetActive(false);
        }

        public void Initialize()
        {
        }

        public void Show()
        {
            Panel.SetActive(true);
        }

        public void SetMessage(string message)
        {
            Message.text = message;
        }
        
        public GameObject Panel;
        public TMP_Text Message;
    }
}