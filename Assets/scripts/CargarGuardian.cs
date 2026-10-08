using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Guardianes
{
    public class CargarGuardian : MonoBehaviour
    {
        public DatosGuardianes catalogo;
        [Tooltip("Para retrato en Canvas, mapa o dialogos")]
        public Image imagenUI;
        public TMP_Text nombreUI;
        [Tooltip("Punto del mundo donde instanciar el prefab jugable; dejar vacio para mostrar solo retrato")]
        public Transform puntoAparicion;
        GameObject instancia;

        void Start()
        {
            int indice = ProgresoGuardianes.Personaje;
            if (catalogo == null || !catalogo.EsValido(indice))
            { Debug.LogWarning("Primero selecciona un personaje en SeleccionPersonaje.", this); return; }
            DatosGuardianes.Personaje datos = catalogo.personajes[indice];
            if (imagenUI != null) { imagenUI.sprite = datos.retrato; imagenUI.preserveAspect = true; }
            if (nombreUI != null) nombreUI.text = datos.nombre;
            if (puntoAparicion != null)
            {
                if (datos.prefabJuego == null)
                { Debug.LogError("Falta el prefab del personaje " + datos.nombre, this); return; }
                instancia = Instantiate(datos.prefabJuego, puntoAparicion.position, puntoAparicion.rotation);
            }
        }
        void OnDestroy() { if (instancia != null) Destroy(instancia); }
    }
}
