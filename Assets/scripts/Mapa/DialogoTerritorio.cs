using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GuardianesTerritorio
{
    public class DialogoTerritorio : MonoBehaviour
    {
        public TMP_Text textoNombre;
        public TMP_Text textoDialogo;
        public Image imagenRetrato;
        public string nombreNPC;
        public Sprite retrato;
        [TextArea(2, 6)] public string[] bloques;
        public PanelActividadTerritorio panel;
        int indice;

        // Conectar en PanelActividadTerritorio.alAbrir.
        public void Comenzar() { indice = 0; Mostrar(); }

        public void Siguiente()
        {
            if (bloques == null || bloques.Length == 0) return;
            indice++;
            if (indice >= bloques.Length)
            {
                if (panel != null) panel.Completar();
                return;
            }
            Mostrar();
        }

        void Mostrar()
        {
            if (textoNombre != null) textoNombre.text = nombreNPC;
            if (imagenRetrato != null)
            {
                imagenRetrato.sprite = retrato;
                imagenRetrato.enabled = retrato != null;
            }
            if (textoDialogo != null)
                textoDialogo.text = bloques != null && bloques.Length > 0 ? bloques[indice] : "Configura los bloques del dialogo en el Inspector.";
        }
    }
}
