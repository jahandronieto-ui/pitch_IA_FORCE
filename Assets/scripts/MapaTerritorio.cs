using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Guardianes
{
    public class MapaTerritorio : MonoBehaviour
    {
        [System.Serializable]
        public class Etapa
        {
            public string nombre;
            public string escena;
            [TextArea] public string retos;
            public Button boton;
            public GameObject candado;
            public GameObject completado;
            public Image[] estrellas = new Image[3];
        }
        public Etapa[] etapas = new Etapa[]
        {
            new Etapa { nombre = "Tolima", escena = "Tolima", retos = "Quemas agricolas\nResiduos en zonas rurales\nColillas" },
            new Etapa { nombre = "Huila", escena = "Huila", retos = "Quema de residuos\nVidrios en el entorno\nPolvora" },
            new Etapa { nombre = "Caqueta", escena = "Caqueta", retos = "Manejo de residuos\nColillas y fogatas\nAlertas comunitarias" },
            new Etapa { nombre = "Putumayo", escena = "Putumayo", retos = "Residuos en areas naturales\nPolvora\nBuen uso del fuego" }
        };
        public Sprite estrellaLlena, estrellaVacia;
        public TMP_Text textoProgreso, textoPorcentaje;
        public Slider barraPrevencion;
        public GameObject panelDetalle;
        public TMP_Text tituloDetalle, retosDetalle;
        public Button botonEntrar;
        public GameObject panelInstrucciones, panelCreditos;
        public GameObject panelConfirmarReinicio;
        public string escenaSeleccion = "SeleccionPersonaje";
        public string escenaInicio = "inicio";
        int etapaElegida = -1;

        void Start()
        {
            if (panelDetalle != null) panelDetalle.SetActive(false);
            if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
            if (panelCreditos != null) panelCreditos.SetActive(false);
            if (panelConfirmarReinicio != null) panelConfirmarReinicio.SetActive(false);
            for (int i = 0; i < etapas.Length; i++)
            {
                int indice = i;
                if (etapas[i] != null && etapas[i].boton != null)
                    etapas[i].boton.onClick.AddListener(() => VerEtapa(indice));
            }
            if (botonEntrar != null) botonEntrar.onClick.AddListener(Entrar);
            Refrescar();
        }

        public void Refrescar()
        {
            for (int i = 0; i < etapas.Length; i++)
            {
                Etapa e = etapas[i]; if (e == null) continue;
                bool abierta = i < 4 && ProgresoGuardianes.Desbloqueada(i);
                int estrellas = ProgresoGuardianes.Estrellas(i);
                if (e.boton != null) e.boton.interactable = abierta;
                if (e.candado != null) e.candado.SetActive(!abierta);
                if (e.completado != null) e.completado.SetActive(estrellas > 0);
                for (int s = 0; s < e.estrellas.Length; s++)
                    if (e.estrellas[s] != null) e.estrellas[s].sprite = s < estrellas ? estrellaLlena : estrellaVacia;
            }
            if (textoProgreso != null) textoProgreso.text = ProgresoGuardianes.Completadas + "/4 etapas";
            if (textoPorcentaje != null) textoPorcentaje.text = Mathf.RoundToInt(ProgresoGuardianes.Porcentaje * 100) + "%";
            if (barraPrevencion != null)
            { barraPrevencion.minValue = 0; barraPrevencion.maxValue = 1; barraPrevencion.value = ProgresoGuardianes.Porcentaje; }
        }

        public void VerEtapa(int indice)
        {
            if (indice < 0 || indice >= etapas.Length || !ProgresoGuardianes.Desbloqueada(indice)) return;
            etapaElegida = indice;
            if (tituloDetalle != null) tituloDetalle.text = etapas[indice].nombre;
            if (retosDetalle != null) retosDetalle.text = etapas[indice].retos;
            if (panelDetalle != null) panelDetalle.SetActive(true);
        }
        public void Entrar()
        {
            if (etapaElegida >= 0 && ProgresoGuardianes.Desbloqueada(etapaElegida)) Cargar(etapas[etapaElegida].escena);
        }
        void Cargar(string escena)
        {
            if (Application.CanStreamedLevelBeLoaded(escena)) SceneManager.LoadSceneAsync(escena);
            else Debug.LogError("Agrega la escena " + escena + " a la compilacion.", this);
        }
        public void CerrarDetalle() { if (panelDetalle != null) panelDetalle.SetActive(false); }
        public void CambiarPersonaje() { Cargar(escenaSeleccion); }
        public void VolverInicio() { Cargar(escenaInicio); }
        public void AbrirInstrucciones() { if (panelInstrucciones != null) panelInstrucciones.SetActive(true); }
        public void CerrarInstrucciones() { if (panelInstrucciones != null) panelInstrucciones.SetActive(false); }
        public void AbrirCreditos() { if (panelCreditos != null) panelCreditos.SetActive(true); }
        public void CerrarCreditos() { if (panelCreditos != null) panelCreditos.SetActive(false); }
        public void PedirReinicio() { if (panelConfirmarReinicio != null) panelConfirmarReinicio.SetActive(true); }
        public void CancelarReinicio() { if (panelConfirmarReinicio != null) panelConfirmarReinicio.SetActive(false); }
        public void ConfirmarReinicio()
        {
            ProgresoGuardianes.ReiniciarViaje(); etapaElegida = -1;
            CerrarDetalle(); CancelarReinicio(); Refrescar();
        }
    }
}
