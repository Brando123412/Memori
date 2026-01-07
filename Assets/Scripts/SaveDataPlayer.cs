using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SaveDataPlayer", menuName = "Scriptable Objects/SaveDataPlayer")]
public class SaveDataPlayer : ScriptableObject
{
    [SerializeField] string namePlayer;
    [SerializeField] string genero;
    public void SetName(string newName)
    {
        namePlayer = newName;
        Debug.Log("Nombre guardado: " + namePlayer);
    }

    public void SetGenero(string newGenero)
    {
        genero = newGenero;
        Debug.Log("Género guardado: " + genero);
    }
    public string GetName() => namePlayer;
    public string GetGenero() => genero;
}
