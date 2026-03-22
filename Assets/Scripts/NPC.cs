using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("References - Behaviour")]
    [SerializeField] private CardHand hand;

    [Header("References - Assets")]
    [SerializeField] private GameObject requestBubble;

    private CowboyState state => GetComponent<CowboyState>();

    public void RequestCard()
    {
        StartCoroutine(IERequestCard());
    }

    public void AnswerRequest(CardRank rank)
    {

    }

    public void TakeCard()
    {

    }

    public void PlacePair()
    {
        int[] ranks = new int[14];
        bool havePair = false;
        for (int i = 0; i < hand.deck.Count; i++)
        {
            int cardIndex = (int)hand.deck[i].rank;
            ranks[cardIndex]++;
            if (ranks[cardIndex] == 2)
            {
                int ind = hand.FindRank(hand.deck, hand.deck[i].rank);
                hand.RemovePair(hand.deck, hand.deck[i], hand.deck[ind]);
                CheatManager.instance.SetCurrentTurnPairDroppedIndeces(i, ind);
                havePair = true;
                break;
            }
        }

        if (havePair)
        {
            GameManager.instance.npcPairs++;
        }
    }

    private void DisplayCardRequest(Card card)
    {
        requestBubble.SetActive(true);
        requestBubble.GetComponent<RequestBubble>().SetRank(card.rank);
    }

    private void HideCardRequest()
    {
        requestBubble.SetActive(false);
    }

    IEnumerator IERequestCard()
    {
        yield return new WaitForSeconds(4);
        int i = Random.Range(0, hand.deck.Count);
        Card card = hand.deck[i];
        CheatManager.instance.SetCurrentTurnRequestRank(card.rank);
        DisplayCardRequest(card);
        GameManager.instance.ChangeTurnPhase(TurnPhase.AnswerRequest);
    }
}
