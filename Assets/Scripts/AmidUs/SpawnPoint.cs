using UnityEngine;

namespace AmidUs
{
    public class SpawnPoint : MonoBehaviour
    {
        private const float radius = 5f;
        public Vector3 GetRandomPosition()
        {
            var randomX = Random.Range(-radius, radius);
            var randomZ = Random.Range(-radius, radius);

            return transform.position + new Vector3(randomX, 0f, randomZ);
        }
    }
}