using UnityEngine;

namespace GuardianesTerritorio
{
    public class CrearJugadorTerritorio : MonoBehaviour
    {
        public const string ClaveSeleccion = "ID_Seleccionada";
        public CatalogoPersonajesTerritorio catalogo;
        public Transform puntoInicio;
        public CamaraTerritorio camara;
        public JugadorTerritorio Jugador { get; private set; }

        void Start()
        {
            if (catalogo == null || puntoInicio == null)
            {
                Debug.LogError("Asigna catalogo y puntoInicio en CrearJugadorTerritorio.", this);
                return;
            }
            if (!PlayerPrefs.HasKey(ClaveSeleccion))
            {
                Debug.LogError("Primero selecciona un personaje. No se ha guardado ID_Seleccionada.", this);
                return;
            }
            int id = PlayerPrefs.GetInt(ClaveSeleccion);
            GameObject prefab = catalogo.Buscar(id);
            if (prefab == null || prefab.GetComponent<JugadorTerritorio>() == null)
            {
                Debug.LogError("El ID " + id + " necesita un prefab con JugadorTerritorio en su raiz.", this);
                return;
            }
            GameObject instancia = Instantiate(prefab, puntoInicio.position, Quaternion.identity);
            Jugador = instancia.GetComponent<JugadorTerritorio>();
            if (camara != null) camara.objetivo = instancia.transform;
        }
    }
}
