using TMPro;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer logoPos1;
    [SerializeField] private SpriteRenderer logoPos2;
    [SerializeField] private SpriteRenderer logoPos3;
    [SerializeField] private TextMeshPro rank1;
    [SerializeField] private TextMeshPro rank2;

    [SerializeField] private Sprite heartLogo;
    [SerializeField] private Sprite spadeLogo;
    [SerializeField] private Sprite clubLogo;
    [SerializeField] private Sprite diamondLogo;
    [SerializeField] private Sprite kingLogo;
    [SerializeField] private Sprite queenLogo;
    [SerializeField] private Sprite jackLogo;

    public bool selected = false;

    public void SetUp(CardRank rank, CardSuit suit)
    {
        switch (suit)
        {
            case CardSuit.NULL:
                break;
            case CardSuit.Hearts:
                logoPos1.sprite = heartLogo;
                logoPos2.sprite = heartLogo;
                logoPos3.sprite = heartLogo;
                break;
            case CardSuit.Diamonds:
                logoPos1.sprite = diamondLogo;
                logoPos2.sprite = diamondLogo;
                logoPos3.sprite = diamondLogo;
                break;
            case CardSuit.Spades:
                logoPos1.sprite = spadeLogo;
                logoPos2.sprite = spadeLogo;
                logoPos3.sprite = spadeLogo;
                break;
            case CardSuit.Clubs:
                logoPos1.sprite = clubLogo;
                logoPos2.sprite = clubLogo;
                logoPos3.sprite = clubLogo;
                break;
            default:
                break;
        }

        if (rank == CardRank.King)
        {
            logoPos3.sprite = kingLogo;
        }
        else if (rank == CardRank.Queen)
        {
            logoPos3.sprite = queenLogo;
        }
        else if (rank == CardRank.Jack)
        {
            logoPos3.sprite = jackLogo;
        }

        rank1.text = rank.ToString();
        rank2.text = rank.ToString();
    }
}
