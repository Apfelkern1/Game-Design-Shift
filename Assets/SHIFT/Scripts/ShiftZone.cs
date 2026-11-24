using UnityEngine;

namespace ShiftPrototype
{
    public class ShiftZone : MonoBehaviour
    {
        public enum ZoneType { Hazard, Checkpoint, Exit }
        public ZoneType type;
        public ShiftWorld world;
        public int room;
        public Transform respawnPoint;
        void OnTriggerEnter2D(Collider2D other)
        {
            ShiftPlayer player = other.GetComponent<ShiftPlayer>();
            if (player == null || world.Completed || world.Room != room) return;
            switch (type)
            {
                case ZoneType.Hazard: world.Respawn(); break;
                case ZoneType.Checkpoint:
                    world.SetCheckpoint(respawnPoint != null ? (Vector2)respawnPoint.position : (Vector2)transform.position);
                    break;
                case ZoneType.Exit: world.ExitRoom(room); break;
            }
        }
    }
}
