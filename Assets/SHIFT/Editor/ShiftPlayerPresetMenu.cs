using UnityEditor;

namespace ShiftPrototype.Editor
{
    // Preset without losing the World-Reference
    // Backup and restore are still possible
    public static class ShiftPlayerPresetMenu
    {
        [MenuItem("CONTEXT/ShiftPlayer/Celeste-Sprungwerte anwenden")]
        static void Apply(MenuCommand command)
        {
            var player = (ShiftPlayer)command.context;
            Undo.RecordObject(player, "Celeste-Sprungwerte anwenden");
            player.ApplyCelesteJumpPreset();
            EditorUtility.SetDirty(player);
        }
    }
}
