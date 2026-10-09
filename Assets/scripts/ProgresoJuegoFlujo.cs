using UnityEngine;

namespace GuardianesFlujo
{
    public static class ProgresoJuegoFlujo
    {
        public static readonly string[] Territorios = { "Tolima", "Huila", "Caqueta", "Putumayo" };
        public static readonly string[] Actividades = { "actividad1", "actividad2", "actividad3", "compromiso" };
        public static bool ActividadCompleta(string territorio, string actividad)
        { return PlayerPrefs.GetInt("GT." + territorio + ".Mision." + actividad, 0) == 1; }
        public static bool JuegoCompleto()
        {
            foreach (string territorio in Territorios)
                if (PlayerPrefs.GetInt("GT." + territorio + ".Completo", 0) != 1) return false;
            return true;
        }
        public static void Reiniciar(bool borrarSeleccion)
        {
            foreach (string territorio in Territorios)
            {
                PlayerPrefs.DeleteKey("GT." + territorio + ".Mision.bienvenida");
                foreach (string actividad in Actividades) PlayerPrefs.DeleteKey("GT." + territorio + ".Mision." + actividad);
                PlayerPrefs.DeleteKey("GT." + territorio + ".Completo");
                PlayerPrefs.DeleteKey("GT." + territorio + ".Desbloqueado");
            }
            PlayerPrefs.DeleteKey("GT.Juego.Completo");
            if (borrarSeleccion) PlayerPrefs.DeleteKey("ID_Seleccionada");
            PlayerPrefs.Save();
        }
    }
}
