#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GuardianesFlujo;

public static class ProyectoGuardianesEditor
{
    [MenuItem("Guardianes/Proyecto/Validar proyecto completo")]
    public static void Validar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var estado = EditorSceneManager.GetSceneManagerSetup();
        var errores = new List<string>();
        try
        {
            foreach (string nombre in new[] { "inicio", "SeleccionPersonaje", "Mapa", "Mundo_Tolima", "Mundo_Huila", "Mundo_Caqueta", "Mundo_Putumayo" })
            {
                string ruta = "Assets/Scenes/" + nombre + ".unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ruta) == null) { errores.Add("Falta " + ruta); continue; }
                var escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
                foreach (var raiz in escena.GetRootGameObjects())
                    foreach (var tr in raiz.GetComponentsInChildren<Transform>(true))
                        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(tr.gameObject) > 0)
                            errores.Add(nombre + ": script perdido en " + tr.name);
                foreach (var raiz in escena.GetRootGameObjects())
                    foreach (var mundo in raiz.GetComponentsInChildren<GuardianesTolima.TolimaMundo>(true))
                    {
                        if (mundo.fondo == null || mundo.fondo.sprite == null || mundo.camara == null || mundo.puntoInicio == null)
                            errores.Add(nombre + ": referencias del mundo incompletas.");
                        if (mundo.personajes == null || mundo.personajes.Length != 4) errores.Add(nombre + ": se requieren cuatro jugadores.");
                        if (mundo.sitios == null || mundo.sitios.Length != 5) errores.Add(nombre + ": se requieren cinco sitios.");
                    }
            }
            foreach (var guid in AssetDatabase.FindAssets("t:DefinicionPersonaje"))
            {
                var p = AssetDatabase.LoadAssetAtPath<DefinicionPersonaje>(AssetDatabase.GUIDToAssetPath(guid));
                if (p.prefab == null || p.retrato == null) errores.Add(p.name + ": falta prefab o retrato.");
                foreach (var sprite in new[] { p.reposoAbajo, p.reposoArriba, p.reposoIzquierda, p.reposoDerecha })
                    if (sprite == null) errores.Add(p.name + ": pose de reposo vacía.");
                foreach (var frames in new[] { p.caminarAbajo, p.caminarArriba, p.caminarIzquierda, p.caminarDerecha })
                {
                    if (frames == null || frames.Length == 0) { errores.Add(p.name + ": faltan frames."); continue; }
                    foreach (var sprite in frames) if (sprite == null) errores.Add(p.name + ": frame perdido.");
                }
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(estado); }
        EditorUtility.DisplayDialog("Validación Guardianes", errores.Count == 0
            ? "Referencias completas. Prueba el flujo, las interacciones y los cuatro personajes en Play."
            : string.Join("\n", errores), "Aceptar");
    }
}
#endif
