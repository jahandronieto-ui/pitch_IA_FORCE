using UnityEngine;

namespace Guardianes
{
    public static class ProgresoGuardianes
    {
        const string Prefijo = "GuardianesTerritorio.";
        public static int Personaje { get { return PlayerPrefs.GetInt(Prefijo + "Personaje", -1); } }
        public static void Elegir(int indice)
        {
            PlayerPrefs.SetInt(Prefijo + "Personaje", indice);
            PlayerPrefs.Save();
        }
        public static int Estrellas(int etapa) { return Mathf.Clamp(PlayerPrefs.GetInt(Prefijo + "Etapa" + etapa, 0), 0, 3); }
        public static bool Desbloqueada(int etapa) { return etapa == 0 || (etapa > 0 && etapa < 4 && Estrellas(etapa - 1) > 0); }
        public static int Completadas
        {
            get { int total = 0; for (int i = 0; i < 4; i++) if (Estrellas(i) > 0) total++; return total; }
        }
        // Porcentaje del maximo de 12 estrellas; no constituye una medicion real de prevencion.
        public static float Porcentaje
        {
            get { int total = 0; for (int i = 0; i < 4; i++) total += Estrellas(i); return total / 12f; }
        }
        public static void Completar(int etapa, int estrellas)
        {
            if (etapa < 0 || etapa >= 4 || !Desbloqueada(etapa)) return;
            PlayerPrefs.SetInt(Prefijo + "Etapa" + etapa, Mathf.Max(Estrellas(etapa), Mathf.Clamp(estrellas, 1, 3)));
            PlayerPrefs.Save();
        }
        public static void ReiniciarViaje()
        {
            for (int i = 0; i < 4; i++) PlayerPrefs.DeleteKey(Prefijo + "Etapa" + i);
            PlayerPrefs.Save();
        }
    }
}
