using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreEntry
{
    public string nombre;
    public int puntaje;

    public ScoreEntry(string nombre, int puntaje)
    {
        this.nombre = nombre;
        this.puntaje = puntaje;
    }
}

[CreateAssetMenu(fileName = "Ordenamiento", menuName = "ScriptableObjects/Ordenamiento", order = 0)]
public class InsertionSort : ScriptableObject
{
    [SerializeField] private List<ScoreEntry> puntajes = new List<ScoreEntry>();
    private int maxEntries = 10;
    private const string PREF_KEY = "ScoreList";

    private void OnEnable()
    {
        CargarDesdePlayerPrefs();
    }

    public void SavePuntaje(string nombre, int puntajeValue)
    {
        // Agrega nuevo puntaje
        if (puntajes.Count < maxEntries)
        {
            puntajes.Add(new ScoreEntry(nombre, puntajeValue));
        }
        else
        {
            // Reemplaza el más bajo si el nuevo es mayor
            if (puntajeValue > puntajes[0].puntaje)
            {
                puntajes[0] = new ScoreEntry(nombre, puntajeValue);
            }
        }

        InsertionSortOrder();
        GuardarEnPlayerPrefs();
    }

    private void InsertionSortOrder()
    {
        for (int i = 1; i < puntajes.Count; i++)
        {
            ScoreEntry temp = puntajes[i];
            int j = i - 1;

            while (j >= 0 && puntajes[j].puntaje > temp.puntaje)
            {
                puntajes[j + 1] = puntajes[j];
                j--;
            }

            puntajes[j + 1] = temp;
        }
    }

    public List<ScoreEntry> ReturnList()
    {
        return puntajes;
    }

    // 🧠 Guarda en PlayerPrefs (en formato JSON)
    private void GuardarEnPlayerPrefs()
    {
        string json = JsonUtility.ToJson(new ScoreListWrapper(puntajes));
        PlayerPrefs.SetString(PREF_KEY, json);
        PlayerPrefs.Save();
        Debug.Log(" Puntajes guardados en PlayerPrefs");
    }

    // 📥 Carga desde PlayerPrefs
    private void CargarDesdePlayerPrefs()
    {
        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            string json = PlayerPrefs.GetString(PREF_KEY);
            ScoreListWrapper data = JsonUtility.FromJson<ScoreListWrapper>(json);

            if (data != null && data.scores != null)
            {
                puntajes = data.scores;
                InsertionSortOrder();
                Debug.Log($" Se cargaron {puntajes.Count} puntajes desde PlayerPrefs");
            }
        }
        else
        {
            puntajes = new List<ScoreEntry>();
        }
    }

    // 🧹 Si quieres reiniciar los puntajes
    public void ResetScores()
    {
        puntajes.Clear();
        PlayerPrefs.DeleteKey(PREF_KEY);
        Debug.Log(" Puntajes reiniciados");
    }

    [System.Serializable]
    private class ScoreListWrapper
    {
        public List<ScoreEntry> scores;
        public ScoreListWrapper(List<ScoreEntry> scores)
        {
            this.scores = scores;
        }
    }
}
