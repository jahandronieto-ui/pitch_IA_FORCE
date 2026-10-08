using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GuardianesFlujo
{
    public static class FlujoUI
    {
        public static RectTransform Rect(string nombre, Transform padre, Vector2 posicion, Vector2 tamano)
        {
            var g = new GameObject(nombre, typeof(RectTransform));
            var r = g.GetComponent<RectTransform>(); r.SetParent(padre, false);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.anchoredPosition = posicion; r.sizeDelta = tamano; return r;
        }
        public static Font Fuente()
        {
            try { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch (Exception) { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }
        public static Text Texto(Transform padre, string texto, Vector2 posicion, Vector2 tamano, int tamaño = 25)
        {
            var t = Rect("Texto", padre, posicion, tamano).gameObject.AddComponent<Text>(); t.font = Fuente(); t.fontSize = tamaño;
            t.text = texto; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false; return t;
        }
        public static Button Boton(Transform padre, string texto, Vector2 posicion, Vector2 tamano, UnityAction accion)
        {
            var r = Rect(texto, padre, posicion, tamano); var imagen = r.gameObject.AddComponent<Image>(); imagen.color = new Color(0.08f, 0.35f, 0.29f, 1);
            var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = imagen; b.onClick.AddListener(accion);
            Texto(r, texto, Vector2.zero, tamano - new Vector2(12, 8)); return b;
        }
        public static Canvas Canvas(string nombre, int orden = 50)
        {
            var g = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = g.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = orden;
            var s = g.GetComponent<CanvasScaler>(); s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; s.referenceResolution = new Vector2(1920, 1080); s.matchWidthOrHeight = 0.5f; return c;
        }
        public static void Mensaje(string titulo, string mensaje)
        {
            var c = Canvas("DialogoFlujo", 100);
            var r = Rect("Velo", c.transform, Vector2.zero, Vector2.zero); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            r.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.82f);
            Texto(r, titulo, new Vector2(0, 130), new Vector2(1050, 80), 36);
            Texto(r, mensaje, Vector2.zero, new Vector2(1050, 170), 28);
            Boton(r, "Aceptar", new Vector2(0, -170), new Vector2(300, 70), () => UnityEngine.Object.Destroy(c.gameObject));
        }
    }
}
