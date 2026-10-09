using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace GuardianesFlujo
{
    [RequireComponent(typeof(Button))]
    public class ClicGuardianes : MonoBehaviour, IPointerDownHandler, ISubmitHandler
    {
        Button boton;
        void Awake() { boton = GetComponent<Button>(); }
        public void OnPointerDown(PointerEventData evento)
        {
            if (evento.button == PointerEventData.InputButton.Left) Sonar();
        }
        public void OnSubmit(BaseEventData evento) { Sonar(); }
        void Sonar()
        {
            if (boton != null && boton.IsActive() && boton.IsInteractable()) SonidosGuardianes.ReproducirClic();
        }
    }
}
