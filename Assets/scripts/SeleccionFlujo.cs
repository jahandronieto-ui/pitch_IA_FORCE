using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GuardianesFlujo
{
    public class SeleccionFlujo : MonoBehaviour
    {
        public CatalogoFlujo catalogo;
        public Button[] botonesPersonaje;
        public Image[] retratos;
        public GameObject[] marcasSeleccion;
        public TMP_Text nombreSeleccionado;
        public Button botonContinuar;
        public string escenaMapa = "Mapa", escenaInicio = "inicio";
        public int idPorDefecto = 1;
        public int[] idsBotones = { 1, 2, 3, 4 };
        int seleccionado;
        Text nombreRuntime;
        void Start()
        {
            if (catalogo == null) { FlujoUI.Mensaje("Seleccion", "No se pudo cargar la lista de personajes."); return; }
            if (nombreSeleccionado == null)
            {
                var canvas = FlujoUI.Canvas("NombrePersonajeSeleccionado");
                nombreRuntime = FlujoUI.Texto(canvas.transform, "", Vector2.zero, new Vector2(650, 60), 28);
                var r = nombreRuntime.rectTransform; r.anchorMin = r.anchorMax = new Vector2(0.5f, 1); r.anchoredPosition = new Vector2(0, -95);
            }
            if (botonesPersonaje != null)
                foreach (var boton in botonesPersonaje)
                    if (boton != null && boton.GetComponent<TarjetaPersonajeFlujo>() == null)
                        boton.gameObject.AddComponent<TarjetaPersonajeFlujo>();
            int id = PlayerPrefs.GetInt("ID_Seleccionada", idPorDefecto);
            if (catalogo.Buscar(id) == null) id = idPorDefecto;
            AplicarSeleccion(id);
        }
        public void Seleccionar(int id)
        {
            if (FlujoTransicion.Ocupada || catalogo == null) return;
            AplicarSeleccion(id);
        }
        void AplicarSeleccion(int id)
        {
            var personaje = catalogo.Buscar(id);
            if (personaje == null || personaje.prefab == null) return;
            if (botonesPersonaje != null)
                for (int i = 0; i < botonesPersonaje.Length; i++)
                {
                    var boton = botonesPersonaje[i];
                    if (boton == null) continue;
                    var tarjeta = boton.GetComponent<TarjetaPersonajeFlujo>();
                    if (tarjeta != null) tarjeta.Seleccionada(i < idsBotones.Length && idsBotones[i] == id);
                }
            seleccionado = id; PlayerPrefs.SetInt("ID_Seleccionada", id); PlayerPrefs.Save();
            for (int i = 0; i < marcasSeleccion.Length; i++)
            {
                if (marcasSeleccion[i] != null) marcasSeleccion[i].SetActive(i < idsBotones.Length && idsBotones[i] == id);
            }
            if (nombreSeleccionado != null) nombreSeleccionado.text = personaje.nombre;
            if (nombreRuntime != null) nombreRuntime.text = personaje.nombre;
            if (botonContinuar != null) botonContinuar.interactable = true;
        }
        public void Continuar()
        {
            if (catalogo == null || catalogo.Buscar(seleccionado) == null) return;
            PlayerPrefs.SetInt("ID_Seleccionada", seleccionado); PlayerPrefs.Save(); FlujoTransicion.Cargar(escenaMapa);
        }
        public void Volver() { FlujoTransicion.Cargar(escenaInicio); }
    }
}
