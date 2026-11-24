using UnityEngine;

namespace ShiftPrototype
{
    public class ShiftWorld : MonoBehaviour
    {
        public GameObject factory;
        public GameObject ruins;
        public ShiftPlayer player;
        public ShiftCamera cameraRig;
        public Transform[] roomSpawns;
        public bool InRuins { get; private set; }
        public bool Completed { get; private set; }
        public int Room { get; private set; }
        public int Deaths { get; private set; }
        public string Message => Time.time < messageUntil ? message : "";
        Vector2 checkpoint;
        bool checkpointRuins;
        string message;
        float messageUntil, lastRespawn = -10f;

        void Start() { Restart(); }
        public void Notify(string text)
        {
            message = text;
            messageUntil = Time.time + 2.5f;
        }
        public void SetEra(bool useRuins)
        {
            InRuins = useRuins;
            factory.SetActive(!useRuins);
            ruins.SetActive(useRuins);
            if (cameraRig != null) cameraRig.SetEra(useRuins);
            Physics2D.SyncTransforms();
        }
        public void TrySwap()
        {
            if (Completed) return;
            // activate target world for a short time so their controller can be checked
            GameObject target = InRuins ? factory : ruins;
            target.SetActive(true);
            Physics2D.SyncTransforms();
            Bounds bounds = player.BodyCollider.bounds;
            Vector2 size = (Vector2)bounds.size - new Vector2(0.04f, 0.04f);
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(bounds.center, size, 0))
            {
                if (!hit.isTrigger && hit.transform.IsChildOf(target.transform))
                {
                    target.SetActive(false);
                    Physics2D.SyncTransforms();
                    Notify("Zeitwechsel blockiert: Hier ist in der anderen Zeit eine Wand.");
                    return;
                }
            }
            SetEra(!InRuins);
        }
        public void SetCheckpoint(Vector2 position)
        {
            checkpoint = position;
            checkpointRuins = InRuins;
            Notify("Checkpoint gespeichert");
        }
        public void Respawn()
        {
            if (Completed || Time.time - lastRespawn < 0.15f) return;
            lastRespawn = Time.time;
            Deaths++;
            SetEra(checkpointRuins);
            player.Teleport(checkpoint);
            cameraRig.Snap();
            Notify("Zurück am Checkpoint");
        }
        public void EnterRoom(int index)
        {
            Room = index;
            SetEra(false);
            checkpoint = roomSpawns[index].position;
            checkpointRuins = false;
            player.Teleport(checkpoint);
            cameraRig.Snap();
        }
        public void ExitRoom(int sourceRoom)
        {
            if (Completed || sourceRoom != Room) return;
            if (Room + 1 < roomSpawns.Length) EnterRoom(Room + 1);
            else { Completed = true; player.Stop(); }
        }
        public void Restart()
        {
            Completed = false;
            Deaths = 0;
            lastRespawn = -10f;
            messageUntil = 0;
            EnterRoom(0);
        }
    }
}
