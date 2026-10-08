using UnityEngine;
using UnityEngine.UI;

namespace GuardianesFlujo
{
    public class InicioFlujo : MonoBehaviour
    {
        public RectTransform logo, hojas, hojas2, personaje, letrero;
        public RectTransform panelInstrucciones, panelConfiguraciones, panelCredito;
        public string escenaJuego = "SeleccionPersonaje";
        public bool animarHojas = true, animarHojas2 = true, animarPersonaje = true;
        public float amplitudHojas = 5, velocidadHojas = 1.8f, amplitudHojas2 = 5, velocidadHojas2 = 1.4f, amplitudPersonaje = 5, velocidadPersonaje = 1.8f;
        Vector2 p1, p2, p3;
        void Start()
        {
            if (hojas != null) p1 = hojas.anchoredPosition; if (hojas2 != null) p2 = hojas2.anchoredPosition; if (personaje != null) p3 = personaje.anchoredPosition;
            PrepararPanel(panelInstrucciones, "Como jugar", "Elige tu personaje, viaja a Tolima y sigue los caminos.\nMuévete con WASD, flechas o los botones de pantalla.\nAcércate a los marcadores y pulsa E o Hablar. Completa las cinco misiones.");
            PrepararPanel(panelCredito, "Guardianes del Territorio", "Una aventura para aprender a cuidar el territorio.");
            PrepararPanel(panelConfiguraciones, "Configuracion", "Usa teclado o controles de pantalla para moverte.");
        }
        void PrepararPanel(RectTransform panel, string titulo, string texto)
        {
            if (panel == null) return;
            // Los paneles enviados estaban vacios. Conserva el arte si luego agregas contenido propio.
            if (panel.childCount == 0)
            {
                var fondo = panel.GetComponent<Image>(); if (fondo == null) fondo = panel.gameObject.AddComponent<Image>(); fondo.color = new Color(0.02f, 0.13f, 0.16f, 0.98f);
                panel.sizeDelta = new Vector2(1100, 650); panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.anchoredPosition = Vector2.zero;
                FlujoUI.Texto(panel, titulo, new Vector2(0, 215), new Vector2(1000, 85), 38);
                FlujoUI.Texto(panel, texto, Vector2.zero, new Vector2(980, 330), 28);
            }
            FlujoUI.Boton(panel, "Cerrar", new Vector2(0, -240), new Vector2(300, 65), CerrarPaneles);
            panel.gameObject.SetActive(false);
        }
        void Update()
        {
            if (animarHojas && hojas != null) hojas.anchoredPosition = p1 + Vector2.up * Mathf.Sin(Time.time * velocidadHojas) * amplitudHojas;
            if (animarHojas2 && hojas2 != null) hojas2.anchoredPosition = p2 + Vector2.up * Mathf.Sin(Time.time * velocidadHojas2) * amplitudHojas2;
            if (animarPersonaje && personaje != null) personaje.anchoredPosition = p3 + Vector2.up * Mathf.Sin(Time.time * velocidadPersonaje) * amplitudPersonaje;
        }
        public void Jugar() { FlujoTransicion.Cargar(escenaJuego); }
        public void CerrarPaneles()
        {
            if (panelInstrucciones != null) panelInstrucciones.gameObject.SetActive(false);
            if (panelCredito != null) panelCredito.gameObject.SetActive(false);
            if (panelConfiguraciones != null) panelConfiguraciones.gameObject.SetActive(false);
        }
        void Abrir(RectTransform panel) { if (FlujoTransicion.Ocupada || panel == null) return; CerrarPaneles(); panel.gameObject.SetActive(true); panel.SetAsLastSibling(); }
        public void AbrirInstrucciones() { Abrir(panelInstrucciones); }
        public void AbrirCreditos() { Abrir(panelCredito); }
        public void AbrirConfiguraciones() { Abrir(panelConfiguraciones); }
    }
}
