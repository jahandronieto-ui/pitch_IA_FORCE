#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GuardianesMundos.Editor
{
    [CustomEditor(typeof(MundoRegional))]
    public class MundoRegionalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var configuracion = (MundoRegional)target;
            if (configuracion.mundoConfigurado == null) return;
            EditorGUILayout.HelpBox("El mapa, NPC y objetos ya estan guardados en la escena. Selecciona el Gestor para ver los puntos de mision y las rutas. La mascara de navegacion limita los pies del jugador al suelo del fondo ilustrado. Los prefabs del jugador aparecen al pulsar Play.", MessageType.Info);
            if (GUILayout.Button("Seleccionar gestor y mostrar rutas")) Selection.activeGameObject = configuracion.mundoConfigurado.gameObject;
            if (GUILayout.Button("Enfocar mapa completo") && SceneView.lastActiveSceneView != null && configuracion.mundoConfigurado.fondo != null)
                SceneView.lastActiveSceneView.Frame(configuracion.mundoConfigurado.fondo.bounds, false);
        }
    }
}
#endif
