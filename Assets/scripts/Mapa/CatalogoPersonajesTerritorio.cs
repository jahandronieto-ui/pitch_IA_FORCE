using System;
using UnityEngine;

namespace GuardianesTerritorio
{
    [CreateAssetMenu(menuName = "Guardianes/Catalogo de personajes")]
    public class CatalogoPersonajesTerritorio : ScriptableObject
    {
        [Serializable]
        public class Entrada
        {
            public int id;
            public string nombre;
            public GameObject prefab;
        }
        public Entrada[] personajes;

        public GameObject Buscar(int id)
        {
            GameObject resultado = null;
            int coincidencias = 0;
            if (personajes == null) return null;
            foreach (Entrada entrada in personajes)
            {
                if (entrada == null || entrada.id != id) continue;
                coincidencias++;
                resultado = entrada.prefab;
            }
            if (coincidencias > 1)
            {
                Debug.LogError("Hay IDs duplicados en el catalogo: " + id, this);
                return null;
            }
            return resultado;
        }
    }
}
