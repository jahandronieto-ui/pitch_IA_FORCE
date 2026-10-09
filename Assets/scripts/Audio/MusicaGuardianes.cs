using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesFlujo
{
    public class MusicaGuardianes : MonoBehaviour
    {
        public static MusicaGuardianes Instancia { get; private set; }
        [Range(0, 1)] public float volumen = 0.22f;
        public float duracionTransicion = 1.2f;
        AudioSource a, b;
        Coroutine transicion;
        string pistaActual;
        string pistaMundo = "MenuMapa";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void LimpiarEstado() { Instancia = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Iniciar()
        {
            if (Instancia != null) return;
            var existente = FindObjectOfType<MusicaGuardianes>();
            if (existente == null) new GameObject("MusicaGlobalGuardianes").AddComponent<MusicaGuardianes>();
        }
        void Awake()
        {
            if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
            Instancia = this; DontDestroyOnLoad(gameObject);
            volumen = PlayerPrefs.GetFloat("Guardianes.Audio.Volumen", volumen);
            a = CrearFuente(); b = CrearFuente();
            SceneManager.sceneLoaded += AlCargarEscena;
            AplicarEscena(SceneManager.GetActiveScene().name);
        }
        AudioSource CrearFuente()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false; s.loop = true; s.spatialBlend = 0; s.volume = 0;
            return s;
        }
        void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            if (modo == LoadSceneMode.Single) AplicarEscena(escena.name);
        }
        void AplicarEscena(string nombre)
        {
            switch (nombre)
            {
                case "Mundo_Tolima": case "Mundo_Huila": pistaMundo = "TolimaHuila"; break;
                case "Mundo_Caqueta": case "Mundo_Putumayo": pistaMundo = "CaquetaPutumayo"; break;
                case "Final_Juego": pistaMundo = "FinalVictoria"; break;
                case "inicio": case "SeleccionPersonaje": case "Mapa": pistaMundo = "MenuMapa"; break;
                default: pistaMundo = "Actividades"; break;
            }
            Reproducir(pistaMundo);
        }
        public void IniciarActividad() { Reproducir("Actividades"); }
        public void TerminarActividad() { Reproducir(pistaMundo); }
        public void CambiarVolumen(float valor)
        {
            volumen = Mathf.Clamp01(valor);
            PlayerPrefs.SetFloat("Guardianes.Audio.Volumen", volumen); PlayerPrefs.Save();
            if (transicion == null) a.volume = volumen;
        }
        public void Reproducir(string nombre)
        {
            if (nombre == pistaActual) return;
            var clip = Resources.Load<AudioClip>("GuardianesMusica/" + nombre);
            if (clip == null) { Debug.LogWarning("No se encontro la musica: " + nombre); return; }
            if (transicion != null) { StopCoroutine(transicion); transicion = null; }
            // Conserva como fuente saliente la que tiene mayor volumen.
            if (b.volume > a.volume) { var temporal = a; a = b; b = temporal; }
            b.Stop(); b.clip = clip; b.volume = 0; b.Play(); pistaActual = nombre;
            transicion = StartCoroutine(Fundir());
        }
        IEnumerator Fundir()
        {
            float inicio = a.volume, tiempo = 0, duracion = Mathf.Max(0.05f, duracionTransicion);
            while (tiempo < duracion)
            {
                tiempo += Time.unscaledDeltaTime; float t = Mathf.Clamp01(tiempo / duracion);
                a.volume = Mathf.Lerp(inicio, 0, t); b.volume = volumen * t;
                yield return null;
            }
            a.Stop(); a.volume = 0; b.volume = volumen;
            var temporal = a; a = b; b = temporal; transicion = null;
        }
        void OnDestroy()
        {
            if (Instancia != this) return;
            SceneManager.sceneLoaded -= AlCargarEscena; Instancia = null;
        }
    }
}
