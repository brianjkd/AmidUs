using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui
{
    public class VoterIcon : MonoBehaviour
    {
        public void Init(Color color)
        {
            _icon.color = color;
        }
        
        [SerializeField] private Image _icon;
    }
}