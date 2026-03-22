using TMPro;
using UnityEngine;

public class RequestBubble : MonoBehaviour
{
    [SerializeField] private TextMeshPro rank;

    public void SetRank(CardRank rank) {
        this.rank.text = rank.ToString() + "?";
    }
}


