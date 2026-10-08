using UnityEngine;

namespace Guardianes
{
    [CreateAssetMenu(menuName = "Guardianes/Catalogo de personajes")]
    public class DatosGuardianes : ScriptableObject
    {
        [System.Serializable]
        public class Personaje
        {
            public string nombre;
            public Sprite retrato;
            public GameObject prefabJuego;
        }
        public Personaje[] personajes = new Personaje[4];
        public bool EsValido(int indice)
        {
            return personajes != null && indice >= 0 && indice < personajes.Length && personajes[indice] != null;
        }
    }
}
