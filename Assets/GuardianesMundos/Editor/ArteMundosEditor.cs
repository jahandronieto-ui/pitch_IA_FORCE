#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuardianesMundos.Editor
{
    public static class ArteMundosEditor
    {
        [Serializable] public class Manifest { public Atlas[] atlases; }
        [Serializable] public class Atlas { public string ruta; public Entrada[] sprites; }
        [Serializable] public class Entrada { public string nombre; public Recorte rect; }
        [Serializable] public class Recorte { public float x, y, width, height; }

        [MenuItem("Guardianes/Arte/Preparar sprites de los mundos")]
        public static void Preparar()
        {
            var archivo = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GuardianesMundos/Arte/CONFIGURACION_ATLAS.json");
            if (archivo == null) { EditorUtility.DisplayDialog("Arte", "Importa la carpeta Arte completa del paquete.", "Aceptar"); return; }
            var lista = JsonUtility.FromJson<Manifest>(archivo.text);
            foreach (var atlas in lista.atlases)
            {
                var importer = AssetImporter.GetAtPath(atlas.ruta) as TextureImporter;
                if (importer == null) { Debug.LogError("No se encontro " + atlas.ruta); continue; }
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = 100;
                importer.isReadable = true; importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false; importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                var datos = new SpriteMetaData[atlas.sprites.Length];
                for (int i = 0; i < datos.Length; i++)
                {
                    var e = atlas.sprites[i]; var r = e.rect;
                    datos[i] = new SpriteMetaData { name = e.nombre, rect = new Rect(r.x, r.y, r.width, r.height), pivot = new Vector2(0.5f, 0.5f), alignment = 0 };
                }
                importer.spritesheet = datos;
                importer.SaveAndReimport();
            }
            EditorUtility.DisplayDialog("Sprites preparados", "Expande Complementos.png en cada carpeta. Los objetos aparecen separados como sprites. Las escenas ya tienen vinculados el fondo y los atlas.", "Aceptar");
        }

        [MenuItem("Guardianes/Arte/Exportar elementos como PNG individuales")]
        public static void Exportar()
        {
            Preparar();
            var archivo = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GuardianesMundos/Arte/CONFIGURACION_ATLAS.json");
            if (archivo == null) return;
            var lista = JsonUtility.FromJson<Manifest>(archivo.text);
            int cantidad = 0;
            foreach (var atlas in lista.atlases)
            {
                var textura = AssetDatabase.LoadAssetAtPath<Texture2D>(atlas.ruta);
                if (textura == null) continue;
                string carpeta = Path.GetDirectoryName(atlas.ruta) + "/ElementosPNG";
                Directory.CreateDirectory(carpeta);
                foreach (var e in atlas.sprites)
                {
                    var r = e.rect; int w = Mathf.RoundToInt(r.width), h = Mathf.RoundToInt(r.height);
                    var salida = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    salida.SetPixels(textura.GetPixels(Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y), w, h)); salida.Apply();
                    File.WriteAllBytes(carpeta + "/" + e.nombre + ".png", salida.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(salida); cantidad++;
                }
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Elementos exportados", cantidad + " PNG individuales con transparencia creados en ElementosPNG de cada mundo y de Compartidos. Las escenas siguen usando los atlas vinculados.", "Aceptar");
        }
    }
}
#endif
