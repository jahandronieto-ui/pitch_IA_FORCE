using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Guardianes
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovimientoGuardian2D : MonoBehaviour
    {
        [System.Serializable]
        public class Poses
        {
            public Sprite quieto;
            public Sprite[] caminar = new Sprite[4];
        }

        public SpriteRenderer imagen;
        public Poses frente = new Poses();
        public Poses espalda = new Poses();
        public Poses izquierda = new Poses();
        public Poses derecha = new Poses();
        [Min(0)] public float velocidad = 3f;
        [Min(1)] public float cuadrosPorSegundo = 8f;
        public bool permitirMovimiento = true;
        [Tooltip("Activa para un joystick que llame EstablecerEntrada. Desactiva para teclado.")]
        public bool entradaExterna;
        public bool ordenarPorAltura = true;
        public int ordenBase = 0;

        Rigidbody2D cuerpo;
        Vector2 entrada, entradaJoystick;
        Poses posesActuales;
        bool estabaCaminando;
        int cuadro;
        float reloj;

        void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            cuerpo.gravityScale = 0;
            cuerpo.constraints |= RigidbodyConstraints2D.FreezeRotation;
            if (imagen == null) imagen = GetComponentInChildren<SpriteRenderer>();
            if (imagen == null)
            { Debug.LogError("Asigna un SpriteRenderer al guardian.", this); enabled = false; return; }
            posesActuales = frente;
            imagen.flipX = false;
            MostrarQuieto();
        }

        void Update()
        {
            entrada = permitirMovimiento ? Vector2.ClampMagnitude(entradaExterna ? entradaJoystick : LeerTeclado(), 1f) : Vector2.zero;
            bool caminando = entrada.sqrMagnitude > 0.001f;
            if (caminando)
            {
                Poses nuevas = Mathf.Abs(entrada.x) > Mathf.Abs(entrada.y)
                    ? (entrada.x > 0 ? derecha : izquierda)
                    : (entrada.y > 0 ? espalda : frente);
                if (nuevas != posesActuales || !estabaCaminando)
                { posesActuales = nuevas; cuadro = 0; reloj = 0; }
                else
                {
                    reloj += Time.deltaTime;
                    float intervalo = 1f / Mathf.Max(1f, cuadrosPorSegundo);
                    if (reloj >= intervalo)
                    {
                        int pasos = Mathf.FloorToInt(reloj / intervalo);
                        reloj -= pasos * intervalo;
                        if (posesActuales.caminar != null && posesActuales.caminar.Length > 0)
                            cuadro = (cuadro + pasos) % posesActuales.caminar.Length;
                    }
                }
                Sprite sprite = posesActuales.caminar != null && posesActuales.caminar.Length > 0
                    ? posesActuales.caminar[cuadro] : null;
                if (sprite != null) imagen.sprite = sprite;
                else MostrarQuieto();
            }
            else { reloj = 0; cuadro = 0; MostrarQuieto(); }
            estabaCaminando = caminando;
        }

        void FixedUpdate()
        {
            // Velocidad fisica para que los Collider2D del escenario detengan al jugador.
            cuerpo.velocity = entrada * velocidad;
        }

        void LateUpdate()
        {
            if (ordenarPorAltura && imagen != null)
                imagen.sortingOrder = ordenBase - Mathf.RoundToInt(transform.position.y * 100f);
        }

        void MostrarQuieto()
        { if (imagen != null && posesActuales != null && posesActuales.quieto != null) imagen.sprite = posesActuales.quieto; }

        Vector2 LeerTeclado()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k == null) return Vector2.zero;
            return new Vector2(
                ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0) - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0),
                ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1 : 0) - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1 : 0));
#elif ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
            return Vector2.zero;
#endif
        }

        public void EstablecerEntrada(Vector2 valor)
        { entradaJoystick = Vector2.ClampMagnitude(valor, 1f); }
        public void Detener()
        {
            permitirMovimiento = false;
            entrada = entradaJoystick = Vector2.zero;
            if (cuerpo != null) cuerpo.velocity = Vector2.zero;
        }
        public void Reanudar() { permitirMovimiento = true; }
        void OnDisable()
        {
            entrada = entradaJoystick = Vector2.zero;
            estabaCaminando = false;
            if (cuerpo != null) cuerpo.velocity = Vector2.zero;
        }
    }
}
