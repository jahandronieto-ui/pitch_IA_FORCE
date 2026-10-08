#if UNITY_EDITOR
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GuardianesFlujo.Editor
{
    [CustomEditor(typeof(DefinicionPersonaje))]
    public class DefinicionPersonajeEditor : UnityEditor.Editor
    {
        int direccion;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var p = (DefinicionPersonaje)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Asigna sprites de cuerpo completo en Reposo y en Caminar. Para seleccionar varios frames, bloquea este Inspector con el candado y seleccionalos en Project. Los retratos del menu son solo una vista provisional. Usa el mismo lienzo y Pixels Per Unit en todos los frames. No necesitas Animator Controller.", MessageType.Info);
            direccion = EditorGUILayout.Popup("Destino de los frames", direccion, new[] { "Abajo", "Arriba", "Izquierda", "Derecha" });
            if (GUILayout.Button("Asignar sprites seleccionados en Project, por nombre"))
            {
                // Lee la selección actual al hacer clic, no modifica los importadores.
                var sprites = Selection.objects.OfType<Sprite>().OrderBy(s => Regex.Replace(s.name, @"\d+", m => m.Value.PadLeft(12, '0'))).ToArray();
                if (sprites.Length == 0) { EditorUtility.DisplayDialog("Sprites", "Expande la textura en Project y selecciona sus sprites individuales. También puedes arrastrarlos directamente a los campos del Inspector.", "Aceptar"); return; }
                Undo.RecordObject(p, "Asignar frames");
                if (direccion == 0) p.caminarAbajo = sprites;
                else if (direccion == 1) p.caminarArriba = sprites;
                else if (direccion == 2) p.caminarIzquierda = sprites;
                else p.caminarDerecha = sprites;
                EditorUtility.SetDirty(p); AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button("Validar frames"))
            {
                var problemas = new System.Collections.Generic.List<string>();
                if (p.reposoAbajo == null) problemas.Add("Asigna Reposo Abajo con el cuerpo completo.");
                var listas = new[] { p.caminarAbajo, p.caminarArriba, p.caminarIzquierda, p.caminarDerecha };
                var nombres = new[] { "Abajo", "Arriba", "Izquierda", "Derecha" };
                Sprite referencia = p.reposoAbajo;
                for (int i = 0; i < listas.Length; i++)
                {
                    var frames = listas[i];
                    if (frames == null || frames.Length < 2) problemas.Add(nombres[i] + ": faltan frames para una caminata animada.");
                    if (frames == null) continue;
                    foreach (var sprite in frames)
                    {
                        if (sprite == null) { problemas.Add(nombres[i] + ": frame vacio."); continue; }
                        if (referencia == null) referencia = sprite;
                        if (!Mathf.Approximately(sprite.pixelsPerUnit, referencia.pixelsPerUnit) || sprite.rect.size != referencia.rect.size)
                            problemas.Add(sprite.name + ": tamaño del lienzo o Pixels Per Unit diferente.");
                    }
                }
                EditorUtility.DisplayDialog("Animacion de " + p.nombre, problemas.Count == 0 ? "Frames completos. Comprueba el orden visual en Play." : string.Join("\n", problemas.Distinct()), "Aceptar");
            }
            if (GUILayout.Button("Seleccionar prefab conectado") && p.prefab != null) EditorGUIUtility.PingObject(p.prefab);
        }
    }
}
#endif
