using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MemoriCard/Deck Config", fileName = "Deck_")]
public class DeckConfig : ScriptableObject
{
    [Tooltip("Cada elemento se duplicará para formar pares.")]
    public List<CardData> uniqueCards = new();

    public List<CardData> GetShuffledPairs()
    {
        var list = new List<CardData>();
        foreach (var c in uniqueCards)
        {
            list.Add(c);
            list.Add(c);
        }

        // Mezcla simple
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
