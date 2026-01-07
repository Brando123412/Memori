using UnityEngine;

[CreateAssetMenu(menuName = "MemoriCard/Card Data", fileName = "Card_")]
public class CardData : ScriptableObject
{
    public string cardId;         // identificador único
    public Sprite frontSprite;    // sprite de frente
    public Color frontTint = Color.white;
}
