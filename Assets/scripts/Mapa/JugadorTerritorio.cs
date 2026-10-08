using UnityEngine;

namespace GuardianesTerritorio
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JugadorTerritorio : MonoBehaviour
    {
        [Min(0)] public float velocidad = 3f;
        [Min(0.1f)] public float radioInteraccion = 1.2f;
        public LayerMask capaInteraccion;
        public Animator animador;
        public bool teclado = true;
        public int Bloqueos { get; private set; }
        Rigidbody2D cuerpo;
        Vector2 entrada;
        Vector2 entradaTactil;
        Vector2 ultimaDireccion = Vector2.down;

        void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            cuerpo.gravityScale = 0;
            cuerpo.freezeRotation = true;
        }

        void Update()
        {
            Vector2 teclas = teclado ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) : Vector2.zero;
            entrada = Bloqueos > 0 ? Vector2.zero : Vector2.ClampMagnitude(teclas + entradaTactil, 1f);
            if (entrada.sqrMagnitude > 0.001f) ultimaDireccion = entrada.normalized;
            if (animador != null)
            {
                animador.SetFloat("Horizontal", ultimaDireccion.x);
                animador.SetFloat("Vertical", ultimaDireccion.y);
                animador.SetFloat("Velocidad", entrada.magnitude);
            }
            if (teclado && Input.GetKeyDown(KeyCode.E)) Interactuar();
        }

        void FixedUpdate()
        {
            cuerpo.velocity = entrada * velocidad;
        }

        public void EntradaTactil(Vector2 valor) { entradaTactil = Vector2.ClampMagnitude(valor, 1); }

        public void Bloquear()
        {
            Bloqueos++;
            entrada = Vector2.zero;
            entradaTactil = Vector2.zero;
            if (cuerpo != null) cuerpo.velocity = Vector2.zero;
        }

        public void Desbloquear() { Bloqueos = Mathf.Max(0, Bloqueos - 1); }

        public void Interactuar()
        {
            if (Bloqueos > 0) return;
            InteraccionTerritorio mejor = null;
            float distancia = float.PositiveInfinity;
            // Incluye triggers incluso si Physics2D.queriesHitTriggers esta desactivado.
            ContactFilter2D filtro = new ContactFilter2D();
            filtro.SetLayerMask(capaInteraccion);
            filtro.useTriggers = true;
            var encontrados = new System.Collections.Generic.List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, radioInteraccion, filtro, encontrados);
            foreach (Collider2D col in encontrados)
            {
                InteraccionTerritorio candidato = col.GetComponentInParent<InteraccionTerritorio>();
                if (candidato == null || !candidato.isActiveAndEnabled || !candidato.gameObject.activeInHierarchy) continue;
                float d = ((Vector2)col.ClosestPoint(transform.position) - (Vector2)transform.position).sqrMagnitude;
                if (d >= distancia) continue;
                mejor = candidato;
                distancia = d;
            }
            if (mejor != null) mejor.Activar(this);
        }

        void OnDisable()
        {
            entrada = entradaTactil = Vector2.zero;
            if (cuerpo != null) cuerpo.velocity = Vector2.zero;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radioInteraccion);
        }
    }
}
