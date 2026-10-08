using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GuardianesTerritorio
{
    // Alternativa opcional al cambio inmediato de escena de BotonPersonaje.
    public class SeleccionPersonajeTerritorio : MonoBehaviour
    {
        public CatalogoPersonajesTerritorio catalogo;
        public string escenaDestino = "MapaGeneral";
        public UnityEvent<int> alSeleccionar;
        public UnityEvent alFaltarSeleccion;
        int idPendiente;
        bool tieneSeleccion;

        public void Seleccionar(int id)
        {
            if (catalogo == null || catalogo.Buscar(id) == null)
            {
                Debug.LogError("ID sin prefab valido: " + id, this);
                return;
            }
            idPendiente = id;
            tieneSeleccion = true;
            PlayerPrefs.SetInt(CrearJugadorTerritorio.ClaveSeleccion, id);
            PlayerPrefs.Save();
            alSeleccionar.Invoke(id);
        }

        public void Continuar()
        {
            if (!tieneSeleccion)
            {
                alFaltarSeleccion.Invoke();
                return;
            }
            if (catalogo == null || catalogo.Buscar(idPendiente) == null ||
                string.IsNullOrWhiteSpace(escenaDestino) || !Application.CanStreamedLevelBeLoaded(escenaDestino))
            {
                Debug.LogError("Revisa catalogo y escenaDestino; agrega la escena a la lista de compilacion.", this);
                return;
            }
            SceneManager.LoadScene(escenaDestino);
        }
    }
}
