using UnityEngine;

namespace ShiftPrototype
{
    [RequireComponent(typeof(Camera))]
    public class ShiftCamera : MonoBehaviour
    {
        public Transform target;
        public float followSpeed = 8f;
        Vector3 Destination => new Vector3(target.position.x + 2f, 3.2f, -10f);
        public void Snap() { if (target != null) transform.position = Destination; }
        void LateUpdate()
        {
            if (target != null)
                transform.position = Vector3.Lerp(transform.position, Destination, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        }
        public void SetEra(bool ruins)
        {
            GetComponent<Camera>().backgroundColor = ruins ? new Color(0.06f, 0.15f, 0.13f) : new Color(0.08f, 0.10f, 0.17f);
        }
    }
}
