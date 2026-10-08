using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace GuardianesTerritorio
{
    public class ProgresoTerritorio : MonoBehaviour
    {
        [Serializable]
        public class Mision
        {
            public string id;
            public string titulo;
            public Toggle casilla;
            public GameObject marcadorPendiente;
            [Range(0, 4)] public int estrellas;
        }
        public string mundoId = "Tolima";
        public string mundoSiguiente = "Huila";
        public Mision[] misiones;
        public TMP_Text textoMisiones;
        public TMP_Text textoEstrellas;
        public UnityEvent alActualizar;
        public UnityEvent alCompletarMundo;
        public int Completadas { get; private set; }
        public int Estrellas { get; private set; }
        public bool MundoCompleto { get; private set; }

        string Clave(string id) { return "GT." + mundoId + ".Mision." + id; }

        void Start() { Actualizar(); }

        public bool EstaCompleta(string id) { return PlayerPrefs.GetInt(Clave(id), 0) == 1; }

        public void Completar(string id)
        {
            bool existe = false;
            if (misiones != null)
                foreach (Mision m in misiones) if (m != null && m.id == id) existe = true;
            if (!existe)
            {
                Debug.LogError("Mision no configurada: " + mundoId + "/" + id, this);
                return;
            }
            if (EstaCompleta(id)) return;
            PlayerPrefs.SetInt(Clave(id), 1);
            PlayerPrefs.Save();
            Actualizar();
        }

        public void Actualizar()
        {
            Completadas = Estrellas = 0;
            int total = 0;
            int maxEstrellas = 0;
            var ids = new System.Collections.Generic.HashSet<string>();
            if (misiones != null)
                foreach (Mision m in misiones)
                {
                    if (m == null || string.IsNullOrWhiteSpace(m.id) || !ids.Add(m.id)) continue;
                    total++;
                    maxEstrellas += m.estrellas;
                    bool completa = EstaCompleta(m.id);
                    if (completa) { Completadas++; Estrellas += m.estrellas; }
                    if (m.casilla != null) m.casilla.SetIsOnWithoutNotify(completa);
                    if (m.marcadorPendiente != null) m.marcadorPendiente.SetActive(!completa);
                }
            if (textoMisiones != null) textoMisiones.text = Completadas + "/" + total;
            if (textoEstrellas != null) textoEstrellas.text = Estrellas + "/" + maxEstrellas;
            bool antes = MundoCompleto;
            MundoCompleto = total > 0 && Completadas == total;
            if (MundoCompleto)
            {
                PlayerPrefs.SetInt("GT." + mundoId + ".Completo", 1);
                if (!string.IsNullOrWhiteSpace(mundoSiguiente)) PlayerPrefs.SetInt("GT." + mundoSiguiente + ".Desbloqueado", 1);
                PlayerPrefs.Save();
            }
            alActualizar.Invoke();
            if (MundoCompleto && !antes) alCompletarMundo.Invoke();
        }

        // Metodo explicito para un boton de reinicio; no borra otros mundos ni la seleccion.
        public void ReiniciarEsteMundo()
        {
            if (misiones != null)
                foreach (Mision m in misiones) if (m != null) PlayerPrefs.DeleteKey(Clave(m.id));
            PlayerPrefs.DeleteKey("GT." + mundoId + ".Completo");
            PlayerPrefs.Save();
            MundoCompleto = false;
            Actualizar();
        }
    }
}
