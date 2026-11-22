using UnityEngine;

namespace RantsMailDelivery
{
    [RequireComponent(typeof(Camera))]
    public class RunnerCamera : MonoBehaviour
    {
        public Transform target;
        public float ahead = 3.5f;
        public float smoothing = 6f;
        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 goal = new Vector3(Mathf.Max(7.5f, target.position.x + ahead), .20f, -10f);
            transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));
        }
        public void Snap()
        {
            if (target != null) transform.position = new Vector3(Mathf.Max(7.5f, target.position.x + ahead), .20f, -10f);
        }
    }
}
