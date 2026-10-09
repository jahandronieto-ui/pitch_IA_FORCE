using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GuardianesFlujo
{
    public class SonidosGuardianes : MonoBehaviour
    {
        public static SonidosGuardianes Instancia { get; private set; }
        [Range(0, 1)] public float volumen = 0.45f;
        AudioSource fuente;
        AudioClip clic, interaccion;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Limpiar() { Instancia = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Iniciar()
        {
            if (Instancia == null && FindObjectOfType<SonidosGuardianes>() == null)
                new GameObject("SonidosGlobalesGuardianes").AddComponent<SonidosGuardianes>();
        }
        void Awake()
        {
            if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
            Instancia = this; DontDestroyOnLoad(gameObject);
            fuente = gameObject.AddComponent<AudioSource>();
            fuente.playOnAwake = false; fuente.spatialBlend = 0;
            clic = Resources.Load<AudioClip>("GuardianesMusica/ClicBoton");
            interaccion = Resources.Load<AudioClip>("GuardianesMusica/Interaccion");
            StartCoroutine(VincularBotones());
        }
        IEnumerator VincularBotones()
        {
            while (true)
            {
                foreach (Button boton in Resources.FindObjectsOfTypeAll<Button>())
                {
                    if (!boton.gameObject.scene.IsValid() || !boton.gameObject.scene.isLoaded) continue;
                    if (boton.GetComponent<ClicGuardianes>() == null) boton.gameObject.AddComponent<ClicGuardianes>();
                }
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }
        public static void ReproducirClic()
        {
            if (Instancia != null && Instancia.clic != null)
                Instancia.fuente.PlayOneShot(Instancia.clic, Instancia.volumen);
        }
        public static void ReproducirInteraccion()
        {
            if (Instancia != null && Instancia.interaccion != null)
                Instancia.fuente.PlayOneShot(Instancia.interaccion, Instancia.volumen);
        }
        void OnDestroy() { if (Instancia == this) Instancia = null; }
    }
}
