using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GuardianesTolima
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TolimaJugadorMotor : MonoBehaviour
    {
        public TolimaMundo mundo;
        public float velocidad = 5f;
        public bool leerTeclado = true;
        public Animator animador;
        Rigidbody2D cuerpo;
        Vector2 entrada, tactil, direccion = Vector2.down;
        bool tieneParametros;
        GuardianesFlujo.PersonajeVisual2D animacionSprites;

        void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            cuerpo.bodyType = RigidbodyType2D.Dynamic;
            cuerpo.gravityScale = 0;
            cuerpo.freezeRotation = true;
            cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
            cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void PrepararAnimator()
        {
            animacionSprites = GetComponent<GuardianesFlujo.PersonajeVisual2D>();
            animador = GetComponentInChildren<Animator>();
            if (animacionSprites != null)
            {
                // Un solo sistema escribe el sprite de cada frame.
                if (animador != null) animador.enabled = false;
                return;
            }
            if (animador == null || animador.runtimeAnimatorController == null) return;
            animador.applyRootMotion = false;
            bool h = false, v = false, s = false;
            foreach (AnimatorControllerParameter p in animador.parameters)
            {
                if (p.type != AnimatorControllerParameterType.Float) continue;
                h |= p.name == "Horizontal"; v |= p.name == "Vertical"; s |= p.name == "Velocidad";
            }
            tieneParametros = h && v && s;
            if (!tieneParametros) Debug.LogWarning("El jugador se movera. Para animarlo agrega los Float Horizontal, Vertical y Velocidad al Animator.", this);
        }

        void Update()
        {
            if (mundo == null) return;
            Vector2 teclas = Vector2.zero;
            bool interactuar = false;
            if (leerTeclado)
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                if (k != null)
                {
                    teclas = new Vector2((k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0),
                        (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0));
                    interactuar = k.eKey.wasPressedThisFrame;
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                teclas = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                interactuar = Input.GetKeyDown(KeyCode.E);
#endif
            }
            entrada = mundo.Bloqueado ? Vector2.zero : Vector2.ClampMagnitude(teclas + tactil, 1);
            if (entrada.sqrMagnitude > 0.001f) direccion = entrada.normalized;
            if (tieneParametros)
            {
                animador.SetFloat("Horizontal", direccion.x);
                animador.SetFloat("Vertical", direccion.y);
                animador.SetFloat("Velocidad", entrada.magnitude);
            }
            if (interactuar && !mundo.Bloqueado) mundo.Interactuar();
        }

        void FixedUpdate()
        {
            if (mundo == null || mundo.Bloqueado) { cuerpo.velocity = Vector2.zero; if (animacionSprites != null) animacionSprites.Movimiento(Vector2.zero); return; }
            Vector2 paso = entrada * velocidad * Time.fixedDeltaTime;
            Vector2 siguiente = cuerpo.position + paso;
            if (!mundo.PuedeCaminar(siguiente))
            {
                Vector2 soloX = cuerpo.position + new Vector2(paso.x, 0);
                Vector2 soloY = cuerpo.position + new Vector2(0, paso.y);
                if (mundo.PuedeCaminar(soloX)) paso.y = 0;
                else if (mundo.PuedeCaminar(soloY)) paso.x = 0;
                else paso = Vector2.zero;
            }
            cuerpo.velocity = paso / Time.fixedDeltaTime;
            if (animacionSprites != null) animacionSprites.Movimiento(cuerpo.velocity);
        }

        public void EntradaTactil(Vector2 valor) { tactil = Vector2.ClampMagnitude(valor, 1); }
        public void Detener() { tactil = entrada = Vector2.zero; if (cuerpo != null) cuerpo.velocity = Vector2.zero; if (animacionSprites != null) animacionSprites.Movimiento(Vector2.zero); }
        void OnDisable() { Detener(); }
    }
}
