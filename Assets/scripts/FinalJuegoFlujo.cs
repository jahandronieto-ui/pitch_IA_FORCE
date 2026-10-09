using UnityEngine;
using UnityEngine.UI;

namespace GuardianesFlujo
{
    public class FinalJuegoFlujo : MonoBehaviour
    {
        public Sprite fondoFinal, estrella;
        public string escenaInicio = "inicio", escenaSeleccion = "SeleccionPersonaje", escenaMapa = "Mapa";
        [Header("Canvas editable de la escena")]
        public bool usarInterfazDeEscena;
        public GameObject tituloVictoria;
        public TMPro.TMP_Text textoEstado;
        public Button botonReiniciar, botonComenzar, botonMapa;
        [Tooltip("Cuatro estrellas por territorio: Tolima, Huila, Caqueta y Putumayo.")]
        public Image[] estrellasResumen = new Image[16];
        void Start()
        {
            Time.timeScale = 1;
            if (usarInterfazDeEscena)
            {
                bool ganado = ProgresoJuegoFlujo.JuegoCompleto();
                if (tituloVictoria != null) tituloVictoria.SetActive(ganado);
                if (textoEstado != null)
                    textoEstado.text = ganado ? "¡Completaste los cuatro territorios!" : "Completa los cuatro mundos para terminar el recorrido.";
                for (int i = 0; i < estrellasResumen.Length && i < 16; i++)
                    if (estrellasResumen[i] != null)
                        estrellasResumen[i].color = ProgresoJuegoFlujo.ActividadCompleta(ProgresoJuegoFlujo.Territorios[i / 4], ProgresoJuegoFlujo.Actividades[i % 4])
                            ? Color.white : new Color(0.25f, 0.28f, 0.32f, 1);
                // Los OnClick quedan vinculados y editables en la propia escena.
                return;
            }
            var canvas = FlujoUI.Canvas("CanvasFinalJuego");
            var fondo = FlujoUI.Rect("Fondo", canvas.transform, Vector2.zero, Vector2.zero);
            fondo.anchorMin = Vector2.zero; fondo.anchorMax = Vector2.one; fondo.offsetMin = fondo.offsetMax = Vector2.zero;
            var imagen = fondo.gameObject.AddComponent<Image>(); imagen.sprite = fondoFinal;
            imagen.color = fondoFinal != null ? new Color(0.25f, 0.35f, 0.3f, 1) : new Color(0.02f, 0.13f, 0.17f, 1);
            imagen.raycastTarget = false;
            var panel = FlujoUI.Rect("PanelResultados", canvas.transform, Vector2.zero, new Vector2(1320, 900));
            panel.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.12f, 0.16f, 0.96f);
            bool completo = ProgresoJuegoFlujo.JuegoCompleto();
            FlujoUI.Texto(panel, completo ? "¡GUARDIÁN DEL TERRITORIO!" : "Tu recorrido por el territorio", new Vector2(0, 335), new Vector2(1220, 90), 44);
            FlujoUI.Texto(panel, completo ? "Completaste Tolima, Huila, Caquetá y Putumayo." : "Completa los cuatro mundos para terminar el juego.", new Vector2(0, 250), new Vector2(1180, 85), 28);
            for (int i = 0; i < ProgresoJuegoFlujo.Territorios.Length; i++)
            {
                string region = ProgresoJuegoFlujo.Territorios[i]; float y = 135 - i * 85;
                FlujoUI.Texto(panel, region == "Caqueta" ? "Caquetá" : region, new Vector2(-260, y), new Vector2(380, 65), 30);
                for (int s = 0; s < 4; s++)
                {
                    var r = FlujoUI.Rect(region + "_Estrella_" + (s + 1), panel, new Vector2(40 + s * 85, y), new Vector2(68, 68));
                    bool lograda = ProgresoJuegoFlujo.ActividadCompleta(region, ProgresoJuegoFlujo.Actividades[s]);
                    if (estrella != null)
                    {
                        var e = r.gameObject.AddComponent<Image>(); e.sprite = estrella; e.preserveAspect = true; e.raycastTarget = false;
                        e.color = lograda ? Color.white : new Color(0.25f, 0.28f, 0.32f, 1);
                    }
                    else FlujoUI.Texto(r, lograda ? "★" : "☆", Vector2.zero, new Vector2(65, 65), 42);
                }
            }
            FlujoUI.Texto(panel, "Reiniciar borra las misiones, estrellas y desbloqueos del recorrido.", new Vector2(0, -235), new Vector2(1200, 60), 23);
            FlujoUI.Boton(panel, "Reiniciar juego", new Vector2(-310, -320), new Vector2(520, 80), ReiniciarJuego);
            FlujoUI.Boton(panel, "Volver a comenzar", new Vector2(310, -320), new Vector2(520, 80), VolverAComenzar);
            FlujoUI.Boton(panel, "Volver al mapa", new Vector2(0, -410), new Vector2(340, 55), () => FlujoTransicion.Cargar(escenaMapa));
        }
        public void ReiniciarJuego() { ReiniciarYViajar(escenaInicio, false); }
        public void VolverAComenzar() { ReiniciarYViajar(escenaSeleccion, true); }
        public void VolverAlMapa() { FlujoTransicion.Cargar(escenaMapa); }
        void ReiniciarYViajar(string escena, bool borrarSeleccion)
        {
            if (FlujoTransicion.Ocupada) return;
            // Solo borra si se pudo iniciar el viaje al destino configurado.
            if (FlujoTransicion.Cargar(escena)) ProgresoJuegoFlujo.Reiniciar(borrarSeleccion);
        }
    }
}
