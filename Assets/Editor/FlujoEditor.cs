#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuardianesFlujo.Editor
{
    public static class FlujoEditor
    {
        [MenuItem("Guardianes/Agregar las cuatro escenas a compilacion")]
        public static void AgregarEscenas()
        {
            var nuevas = new List<EditorBuildSettingsScene>();
            foreach (string nombre in new[] { "inicio", "SeleccionPersonaje", "Mapa", "Mundo_Tolima" })
            {
                var rutas = new List<string>();
                foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
                {
                    string ruta = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(ruta) == nombre) rutas.Add(ruta);
                }
                if (rutas.Count != 1)
                {
                    EditorUtility.DisplayDialog("Escenas", nombre + ": se encontraron " + rutas.Count + " escenas. Reemplaza las originales sin crear duplicados y usa el nombre exacto.", "Aceptar"); return;
                }
                nuevas.Add(new EditorBuildSettingsScene(rutas[0], true));
            }
            // Mantiene otros mundos o escenas ya agregadas al proyecto.
            foreach (var anterior in EditorBuildSettings.scenes)
                if (!nuevas.Exists(e => e.path == anterior.path)) nuevas.Add(anterior);
            EditorBuildSettings.scenes = nuevas.ToArray();
            EditorUtility.DisplayDialog("Escenas conectadas", "Orden: inicio, SeleccionPersonaje, Mapa y Mundo_Tolima. Abre inicio y pulsa Play.", "Aceptar");
        }
        [MenuItem("Guardianes/Validar personajes del flujo")]
        public static void Validar()
        {
            var errores = new List<string>();
            var catalogos = AssetDatabase.FindAssets("t:CatalogoFlujo");
            if (catalogos.Length != 1) errores.Add("Debe existir un CatalogoFlujo para este paquete.");
            foreach (var guid in catalogos)
            {
                var catalogo = AssetDatabase.LoadAssetAtPath<CatalogoFlujo>(AssetDatabase.GUIDToAssetPath(guid));
                var ids = new HashSet<int>();
                foreach (var p in catalogo.personajes)
                {
                    if (p == null) { errores.Add("Definicion vacia."); continue; }
                    if (!ids.Add(p.id)) errores.Add("ID repetido: " + p.id);
                    if (p.prefab == null) errores.Add(p.nombre + ": falta prefab.");
                    else
                    {
                        var visual = p.prefab.GetComponent<PersonajeVisual2D>();
                        if (visual == null || visual.definicion != p) errores.Add(p.nombre + ": prefab sin su definicion correcta.");
                    }
                    if (p.retrato == null) errores.Add(p.nombre + ": retrato original no encontrado. Conserva sus .meta.");
                    if (p.reposoAbajo == null) errores.Add(p.nombre + ": falta asignar pose de cuerpo completo.");
                }
            }
            EditorUtility.DisplayDialog("Personajes", errores.Count == 0 ? "Referencias principales completas. Revisa los frames de cada personaje y prueba las cuatro escenas en Play." : string.Join("\n", errores), "Aceptar");
        }
    }
}
#endif
