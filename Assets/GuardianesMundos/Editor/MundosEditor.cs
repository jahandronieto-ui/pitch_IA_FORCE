#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace GuardianesMundos.Editor
{
    public static class MundosEditor
    {
        [MenuItem("Guardianes/Agregar los siete escenarios a compilacion")]
        public static void Agregar()
        {
            var nuevas = new List<EditorBuildSettingsScene>();
            foreach (string nombre in new[] { "inicio", "SeleccionPersonaje", "Mapa", "Mundo_Tolima", "Mundo_Huila", "Mundo_Caqueta", "Mundo_Putumayo" })
            {
                var coincidencias = new List<string>();
                foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(path) == nombre) coincidencias.Add(path);
                }
                if (coincidencias.Count != 1) { EditorUtility.DisplayDialog("Escenas", nombre + ": se encontraron " + coincidencias.Count + " archivos. Conserva una sola escena con ese nombre.", "Aceptar"); return; }
                nuevas.Add(new EditorBuildSettingsScene(coincidencias[0], true));
            }
            foreach (var anterior in EditorBuildSettings.scenes) if (!nuevas.Exists(e => e.path == anterior.path)) nuevas.Add(anterior);
            EditorBuildSettings.scenes = nuevas.ToArray();
            EditorUtility.DisplayDialog("Mundos", "Los siete escenarios quedaron habilitados. Prueba desde inicio o abre un mundo directamente para revisarlo.", "Aceptar");
        }
    }
}
#endif
