using UnityEngine;

namespace RantsMailDelivery
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupItem : MonoBehaviour
    {
        public bool isBlue;
        public bool everCollected;
        private float originY;
        private void Start() { originY = transform.position.y; }
        private void Update()
        {
            if (GameDirector.Instance != null && GameDirector.Instance.IsPlaying)
                transform.position = new Vector3(transform.position.x, originY + Mathf.Sin(Time.time * 4 + transform.position.x) * .09f, transform.position.z);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<RunnerPlayer>()) return;
            if (GameDirector.Instance == null || !GameDirector.Instance.IsPlaying) return;
            everCollected = true;
            GameDirector.Instance.AddCrystal(isBlue);
            gameObject.SetActive(false);
        }
        public void RestoreAfterCheckpoint(float checkpoint)
        {
            if (transform.position.x >= checkpoint - .01f && (!isBlue || !everCollected)) gameObject.SetActive(true);
        }
    }
}
