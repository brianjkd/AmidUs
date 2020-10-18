using UnityEngine;

namespace AmidUs
{
    public class Nameplate : MonoBehaviour
    {
        void Update()
        {
            transform.eulerAngles = new Vector3(90f, 0f, 0f);
        }

        public void SetName(string name, Color color)
        {
            _textMesh.text = name;
            _textMesh.color = color;
        }
        
        [SerializeField] private TextMesh _textMesh;
    }
}