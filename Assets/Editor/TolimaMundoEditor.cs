#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GuardianesTolima.Editor
{
    [CustomEditor(typeof(TolimaMundo))]
    public class TolimaMundoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            TolimaMundo mundo = (TolimaMundo)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Esta escena necesita tus sprites y prefabs originales. Las interacciones y los paneles se construyen al pulsar Play. Los caminos se ajustan en Rutas usando coordenadas 0–1 del fondo.", MessageType.Info);
            if (GUILayout.Button("Conectar CatalogoFlujo")) Conectar(mundo);
            if (GUILayout.Button("Agregar Tolima y mapa a Build Settings")) AgregarEscenas(mundo);
            if (GUILayout.Button("Validar configuración")) Validar(mundo);
        }

        static void Conectar(TolimaMundo mundo)
        {
            string[] catalogos = AssetDatabase.FindAssets("t:CatalogoFlujo");
            var validos = new List<ScriptableObject>();
            foreach (string guid in catalogos)
            {
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) validos.Add(asset);
            }
            if (validos.Count != 1)
            {
                EditorUtility.DisplayDialog("Catálogo", validos.Count == 0 ?
                    "No se encontro un CatalogoFlujo. Importa Assets/GuardianesFlujo del paquete." :
                    "Hay varios catálogos. Para evitar elegir el incorrecto, configura Personajes manualmente usando el catálogo que corresponde a tu selección.", "Aceptar");
                return;
            }
            SerializedObject origen = new SerializedObject(validos[0]);
            SerializedProperty entradas = origen.FindProperty("personajes");
            if (entradas == null || !entradas.isArray) return;
            var nuevos = new List<TolimaMundo.Personaje>();
            var ids = new HashSet<int>();
            for (int i = 0; i < entradas.arraySize; i++)
            {
                ScriptableObject definicion = entradas.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
                if (definicion == null) { EditorUtility.DisplayDialog("Catalogo", "Hay una definicion vacia.", "Aceptar"); return; }
                SerializedObject entrada = new SerializedObject(definicion);
                SerializedProperty id = entrada.FindProperty("id"), prefab = entrada.FindProperty("prefab");
                if (id == null || prefab == null || prefab.objectReferenceValue == null || !ids.Add(id.intValue))
                {
                    EditorUtility.DisplayDialog("Revisa el catálogo", "Hay IDs duplicados, campos incompletos o prefabs sin asignar. Corrige el catálogo y vuelve a conectar.", "Aceptar");
                    return;
                }
                nuevos.Add(new TolimaMundo.Personaje { id = id.intValue, prefab = prefab.objectReferenceValue as GameObject });
            }
            if (nuevos.Count == 0) return;
            Undo.RecordObject(mundo, "Conectar personajes de Tolima");
            mundo.personajes = nuevos.ToArray();
            EditorUtility.SetDirty(mundo);
            EditorSceneManager.MarkSceneDirty(mundo.gameObject.scene);
            EditorUtility.DisplayDialog("Personajes conectados", nuevos.Count + " prefabs relacionados con sus ID. Guarda la escena y prueba desde tu selección.", "Aceptar");
        }

        static void AgregarEscenas(TolimaMundo mundo)
        {
            if (string.IsNullOrEmpty(mundo.gameObject.scene.path)) return;
            var escenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            Agregar(escenas, mundo.gameObject.scene.path);
            if (!string.IsNullOrWhiteSpace(mundo.escenaMapa))
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(path) == mundo.escenaMapa) Agregar(escenas, path);
                }
            }
            EditorBuildSettings.scenes = escenas.ToArray();
            EditorUtility.DisplayDialog("Escenas de compilación", "Se agregó Tolima y se habilitó el mapa si su nombre se encontró. Revisa la lista si existen escenas con nombres repetidos.", "Aceptar");
        }
        static void Agregar(List<EditorBuildSettingsScene> lista, string path)
        {
            foreach (EditorBuildSettingsScene escena in lista)
                if (escena.path == path) { escena.enabled = true; return; }
            lista.Add(new EditorBuildSettingsScene(path, true));
        }
        static void Validar(TolimaMundo mundo)
        {
            var fallos = new List<string>();
            if (mundo.fondo == null || mundo.fondo.sprite == null) fallos.Add("Fondo sin sprite: importa en el mismo proyecto original.");
            if (mundo.camara == null) fallos.Add("Falta Main Camera.");
            if (mundo.puntoInicio == null) fallos.Add("Falta punto de inicio.");
            if (mundo.personajes == null || mundo.personajes.Length == 0) fallos.Add("Conecta el catálogo o asigna tus prefabs.");
            else
            {
                var ids = new HashSet<int>();
                foreach (TolimaMundo.Personaje p in mundo.personajes)
                {
                    if (p == null || p.prefab == null) { fallos.Add("Una entrada de personaje no tiene prefab."); continue; }
                    if (!ids.Add(p.id)) fallos.Add("ID duplicado: " + p.id);
                    if (p.prefab.GetComponentInChildren<SpriteRenderer>() == null && p.prefab.GetComponent<GuardianesFlujo.PersonajeVisual2D>() == null) fallos.Add("El prefab " + p.prefab.name + " necesita SpriteRenderer o PersonajeVisual2D.");
                }
            }
            if (mundo.sitios == null || mundo.sitios.Length != 5) fallos.Add("Se necesitan cinco sitios de misión.");
            if (mundo.rutas == null || mundo.rutas.Length == 0) fallos.Add("No hay rutas transitables.");
            EditorUtility.DisplayDialog("Validación Tolima", fallos.Count == 0 ?
                "Las referencias principales están completas. Prueba movimiento, animaciones y cada actividad en Play." : string.Join("\n", fallos), "Aceptar");
        }
    }
}
#endif
