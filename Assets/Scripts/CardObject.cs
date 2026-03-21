using TMPro;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    [SerializeField] private Sprite logoPos1;
    [SerializeField] private Sprite logoPos2;
    [SerializeField] private TextMeshPro rank1;
    [SerializeField] private TextMeshPro rank2;

    [SerializeField] private Sprite background;
    [SerializeField] private Sprite heartLogo;
    [SerializeField] private Sprite spadeLogo;
    [SerializeField] private Sprite clubLogo;
    [SerializeField] private Sprite diamondLogo;

    public void SetUp(CardRank rank, CardSuit suit)
    {

    }
}
