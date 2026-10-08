using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GuardianesTerritorio
{
    public class ViajarTerritorio : MonoBehaviour
    {
        public string escenaDestino;
        [Tooltip("Vacio para mapa general. Tolima se considera abierto inicialmente.")]
        public string mundoRequerido;
        public UnityEvent alEstarBloqueado;

        public void Viajar()
        {
            if (!string.IsNullOrWhiteSpace(mundoRequerido) && mundoRequerido != "Tolima" &&
                PlayerPrefs.GetInt("GT." + mundoRequerido + ".Desbloqueado", 0) != 1)
            {
                alEstarBloqueado.Invoke();
                return;
            }
            if (string.IsNullOrWhiteSpace(escenaDestino) || !Application.CanStreamedLevelBeLoaded(escenaDestino))
            {
                Debug.LogError("Agrega escenaDestino a la lista de escenas de compilacion: " + escenaDestino, this);
                return;
            }
            SceneManager.LoadScene(escenaDestino);
        }
    }
}
