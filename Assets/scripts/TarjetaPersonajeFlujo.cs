using UnityEngine;
using UnityEngine.EventSystems;

namespace GuardianesFlujo
{
    public class TarjetaPersonajeFlujo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Vector3 escalaInicial;
        bool encima, seleccionada;
        void Awake() { escalaInicial = transform.localScale; }
        public void Seleccionada(bool valor) { seleccionada = valor; }
        public void OnPointerEnter(PointerEventData e) { encima = true; }
        public void OnPointerExit(PointerEventData e) { encima = false; }
        void Update()
        {
            float aumento = encima ? 1.06f : seleccionada ? 1.025f : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, escalaInicial * aumento, 1 - Mathf.Exp(-14 * Time.unscaledDeltaTime));
        }
        void OnDisable() { encima = false; transform.localScale = escalaInicial; }
    }
}
