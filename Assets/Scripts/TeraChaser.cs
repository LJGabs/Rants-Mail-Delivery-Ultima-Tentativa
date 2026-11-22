using UnityEngine;

namespace RantsMailDelivery
{
    /// <summary>Tera surge na Parte 3 e se aproxima quando Rant fica cansado.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TeraChaser : MonoBehaviour
    {
        public Transform target;
        private SpriteRenderer visual;
        private void Awake() { visual = GetComponent<SpriteRenderer>(); }
        private void LateUpdate()
        {
            var game = GameDirector.Instance;
            if (target == null || game == null) return;
            bool visible = game.IsPlaying && game.CurrentPart == 2;
            visual.enabled = visible;
            if (!visible) return;
            float gap = game.IsTired ? 2.9f : 4.3f;
            Vector3 desired = new Vector3(target.position.x - gap, -.39f, 1f);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-5f * Time.deltaTime));
        }
    }
}
