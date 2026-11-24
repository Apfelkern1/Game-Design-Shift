using UnityEngine;

namespace ShiftPrototype
{
    // Prototype UI
    public class ShiftHUD : MonoBehaviour
    {
        public ShiftWorld world;
        GUIStyle title, text;
        public string[] roomNames = { "01 / Zeitwechsel", "02 / Sprünge", "03 / Präzise Sprünge" };
        public string[] hints = {
            "Wenn der Weg versperrt ist, kannst du mit E die Zeit wechseln.",
            "Halte Leertaste um höher zu springen."
        };
        string RoomName(int index) => roomNames != null && index >= 0 && index < roomNames.Length
            ? roomNames[index] : "Raum " + (index + 1);
        string RoomHint(int index) => hints != null && index >= 0 && index < hints.Length
            ? hints[index] : "Finde den Ausgang. E wechselt die Zeit.";
        void OnGUI()
        {
            if (world == null || world.player == null) return;
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
                text = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
                title.normal.textColor = text.normal.textColor = Color.white;
            }
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            GUI.Box(new Rect(20, 20, 1240, 112), GUIContent.none);
            GUI.Label(new Rect(36, 28, 650, 36), "SHIFT  /  " + RoomName(world.Room), title);
            GUI.Label(new Rect(36, 68, 650, 28), "Zeit: " + (world.InRuins ? "Ruine" : "Fabrik") + "     |     Tode: " + world.Deaths, text);
            GUI.Box(new Rect(20, 592, 1240, 108), GUIContent.none);
            GUI.Label(new Rect(36, 602, 1200, 48), RoomHint(world.Room), text);
            GUI.Label(new Rect(36, 656, 1200, 30), "A/D: Laufen    Leertaste: Springen    E: Zeitwechsel    R: Respawn", text);
            GUI.Label(new Rect(36, 145, 1100, 55), world.Message, text);
            if (world.Completed)
            {
                GUI.Box(new Rect(320, 250, 640, 210), GUIContent.none);
                GUI.Label(new Rect(360, 280, 570, 45), "Aus der Fabrik entkommen.", title);
                GUI.Label(new Rect(360, 340, 560, 90), "Alle " + world.roomSpawns.Length + " Räume geschafft!\nTode: " + world.Deaths + "\nR startet das Spiel neu.", text);
            }
            GUI.matrix = previous;
        }
    }
}
