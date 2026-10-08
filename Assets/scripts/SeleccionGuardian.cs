using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Guardianes
{
    public class SeleccionGuardian : MonoBehaviour
    {
        public DatosGuardianes catalogo;
        public Button[] botonesPersonaje = new Button[4];
        public Image[] retratos = new Image[4];
        public GameObject[] marcasSeleccion = new GameObject[4];
        public TMP_Text nombreSeleccionado;
        public Button botonContinuar;
        public string escenaMapa = "MapaTerritorio";
        public string escenaInicio = "inicio";
        int seleccionado = -1;

        void Start()
        {
            for (int i = 0; i < botonesPersonaje.Length; i++)
            {
                int indice = i;
                bool valido = catalogo != null && catalogo.EsValido(i);
                if (botonesPersonaje[i] != null)
                {
                    botonesPersonaje[i].interactable = valido;
                    botonesPersonaje[i].onClick.AddListener(() => Seleccionar(indice));
                }
                if (valido && i < retratos.Length && retratos[i] != null)
                {
                    retratos[i].sprite = catalogo.personajes[i].retrato;
                    retratos[i].preserveAspect = true;
                }
            }
            if (botonContinuar != null) botonContinuar.onClick.AddListener(Continuar);
            if (catalogo != null && catalogo.EsValido(ProgresoGuardianes.Personaje))
                seleccionado = ProgresoGuardianes.Personaje;
            Actualizar();
        }

        public void Seleccionar(int indice)
        {
            if (catalogo == null || !catalogo.EsValido(indice)) return;
            seleccionado = indice;
            Actualizar();
        }

        void Actualizar()
        {
            for (int i = 0; i < marcasSeleccion.Length; i++)
                if (marcasSeleccion[i] != null) marcasSeleccion[i].SetActive(i == seleccionado);
            if (nombreSeleccionado != null)
                nombreSeleccionado.text = seleccionado < 0 ? "Selecciona tu guardian" : catalogo.personajes[seleccionado].nombre;
            if (botonContinuar != null) botonContinuar.interactable = seleccionado >= 0;
        }

        public void Continuar()
        {
            if (catalogo == null || !catalogo.EsValido(seleccionado)) return;
            if (!Application.CanStreamedLevelBeLoaded(escenaMapa))
            { Debug.LogError("Agrega la escena " + escenaMapa + " a la compilacion.", this); return; }
            ProgresoGuardianes.Elegir(seleccionado);
            SceneManager.LoadSceneAsync(escenaMapa);
        }

        public void Volver()
        {
            if (Application.CanStreamedLevelBeLoaded(escenaInicio)) SceneManager.LoadSceneAsync(escenaInicio);
            else Debug.LogError("Agrega la escena " + escenaInicio + " a la compilacion.", this);
        }
    }
}
