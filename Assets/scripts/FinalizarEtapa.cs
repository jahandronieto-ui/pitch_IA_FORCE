using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guardianes
{
    public class FinalizarEtapa : MonoBehaviour
    {
        [Range(0, 3)] public int indiceEtapa;
        [Range(1, 3)] public int estrellasGanadas = 3;
        public string escenaMapa = "MapaTerritorio";
        public void GuardarResultado(int estrellas)
        { ProgresoGuardianes.Completar(indiceEtapa, estrellas); }
        public void CompletarYVolver()
        { GuardarResultado(estrellasGanadas); VolverAlMapa(); }
        public void VolverAlMapa()
        {
            if (Application.CanStreamedLevelBeLoaded(escenaMapa)) SceneManager.LoadSceneAsync(escenaMapa);
            else Debug.LogError("Agrega la escena " + escenaMapa + " a la compilacion.", this);
        }
    }
}
