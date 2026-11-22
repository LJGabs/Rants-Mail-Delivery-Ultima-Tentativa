using UnityEngine;

namespace RantsMailDelivery
{
    [RequireComponent(typeof(Collider2D))]
    public class HazardObject : MonoBehaviour
    {
        public bool breakable;
        public bool instantlyFatal;
        public bool risingTongue;
        private Vector3 startPosition;
        private void Awake() { startPosition = transform.position; }
        private void Update()
        {
            if (risingTongue && GameDirector.Instance != null && GameDirector.Instance.IsPlaying)
            {
                float wave = (Mathf.Sin(Time.time * 2.35f + startPosition.x) + 1f) * .5f;
                transform.position = startPosition + Vector3.up * (wave * 1.15f - .5f);
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<RunnerPlayer>();
            if (player == null || GameDirector.Instance == null || !GameDirector.Instance.IsPlaying) return;
            if (breakable && player.IsRolling)
            {
                gameObject.SetActive(false);
                return;
            }
            GameDirector.Instance.OnHazardHit(instantlyFatal);
        }
        public void RestoreAfterCheckpoint(float checkpoint)
        {
            transform.position = startPosition;
            if (startPosition.x >= checkpoint - .01f) gameObject.SetActive(true);
        }
    }
}
