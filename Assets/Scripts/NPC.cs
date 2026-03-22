using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("References - Behaviour")]
    [SerializeField] private CardHand hand;
    [SerializeField] private NPCHand npcHand;

    [Header("References - Assets")]
    [SerializeField] private GameObject requestBubble;
    [SerializeField] private GameObject goFishBubble;

    private CowboyState state => GetComponent<CowboyState>();

    public void RequestCard()
    {
        StartCoroutine(IERequestCard());
    }

    public void AnswerRequest(CardRank rank)
    {
        StartCoroutine(IEAnswerRequest(rank));
    }

    public void TakeCard()
    {
        StartCoroutine (IETakeCard());
    }

    public void PlacePair()
    {
        StartCoroutine(IEPlacePair());
    }

    private void DisplayCardRequest(Card card)
    {
        requestBubble.SetActive(true);
        requestBubble.GetComponent<RequestBubble>().SetRank(card.rank);
    }

    private void DisplayGoFish()
    {
        goFishBubble.SetActive(true);
    }

    IEnumerator IEAnswerRequest(CardRank rank)
    {
        yield return new WaitForSeconds(3);

        int ind = hand.FindRank(hand.deck, rank);
        if (ind == -1)
        {
            DisplayGoFish();
            GameManager.instance.GoFish();
            CheatManager.instance.SetCurrentTurnCardGiven(false);
            CheatManager.instance.SetCurrentTurnRequestInOppositeHand(false);
            GameManager.instance.ChangeTurnPhase(TurnPhase.TakeCard);
            yield return null;
        }

        bool willLie = Random.value < 0.5f;
        if (willLie)
        {
            DisplayGoFish();
            GameManager.instance.GoFish();
            CheatManager.instance.SetCurrentTurnCardGiven(false);
            CheatManager.instance.SetCurrentTurnRequestInOppositeHand(true);
        }
        else
        {
            Card c = Card.Copy(hand.deck[ind]);
            hand.deck.RemoveAt(ind);
            GameManager.instance.GiveCard(c);
            CheatManager.instance.SetCurrentTurnCardGiven(true);
            CheatManager.instance.SetCurrentTurnRequestInOppositeHand(true);
        }

        GameManager.instance.ChangeTurnPhase(TurnPhase.TakeCard);
    }

    IEnumerator IERequestCard()
    {
        yield return new WaitForSeconds(3);
        int i = Random.Range(0, hand.deck.Count);
        Card card = Card.Copy(hand.deck[i]);
        GameManager.instance.request = card;
        CheatManager.instance.SetCurrentTurnRequestRank(card.rank);
        DisplayCardRequest(card);
        GameManager.instance.ChangeTurnPhase(TurnPhase.AnswerRequest);
    }

    IEnumerator IETakeCard()
    {
        yield return new WaitForSeconds(3);
        int i = Random.Range(0, hand.deck.Count);
        Card givenCard = Card.Copy(GameManager.instance.requestAnswer);
        hand.deck.Insert(i, givenCard);
        npcHand.AddCard(i);
        CheatManager.instance.SetCurrentTurnAddedCardIndex(i);
        GameManager.instance.ChangeTurnPhase(TurnPhase.PlacePair);
    }

    IEnumerator IEPlacePair()
    {
        yield return new WaitForSeconds(3);
        int[] ranks = new int[14];
        bool havePair = false;
        bool willLie = Random.value < 0.5f;
        if (!willLie)
        {
            int ind = CheatManager.instance.GetCurrentTurnAddedCardIndex();
            Card addedCard = hand.deck[ind];
            for (int i = 0; i < hand.deck.Count; i++)
            {
                if (i == ind) continue;
                if (hand.deck[i].rank == addedCard.rank)
                {
                    npcHand.RemoveCard(ind);
                    npcHand.RemoveCard(i);
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
            else
            {
                CheatManager.instance.SetCurrentTurnPairDroppedIndeces(-1, -1);
            }

            GameManager.instance.ChangeTurnPhase(TurnPhase.RequestCard);
            yield return null;
        }

        //Lying drop the first pair he has regardless of last OR
        for (int i = 0; i < hand.deck.Count; i++)
        {
            int cardIndex = (int)hand.deck[i].rank;
            ranks[cardIndex]++;
            if (ranks[cardIndex] == 2)
            {
                int ind = hand.FindRank(hand.deck, hand.deck[i].rank);
                npcHand.RemoveCard(ind);
                npcHand.RemoveCard(i);
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
        else
        {
            CheatManager.instance.SetCurrentTurnPairDroppedIndeces(-1, -1);
        }

        GameManager.instance.ChangeTurnPhase(TurnPhase.RequestCard);
    }
}
