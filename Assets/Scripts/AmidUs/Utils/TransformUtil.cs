using System.Collections.Generic;
using UnityEngine;

namespace AmidUs.Utils
{
    public static class TransformUtil
    {
        public static T GetNearest<T>(Vector3 sourcePosition, IEnumerable<T> nearbyTransforms, float minRange) where T : MonoBehaviour
        {
            T nearest = default;
            var nearestDistance = minRange * minRange; // Mathf.Infinity;
            
            foreach (var transform in nearbyTransforms)
            {
                var directionToTarget = transform.transform.position - sourcePosition;
                var dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < nearestDistance)
                {
                    nearestDistance = dSqrToTarget;
                    nearest = transform;
                }
            }

            return nearest;
        }
        
        public static Quaternion GetRotationToLookAtTarget(Vector3 source, Vector3 target)
        {
            var relativePos = target - source;
            
            return Quaternion.LookRotation(relativePos, Vector3.up);
        }
    }
}