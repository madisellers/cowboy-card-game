using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CardHand : MonoBehaviour
{
    private const int MAX_RANKS = 13;
    private const int MAX_SUITS = 4;

    private const int EASY_RANK_LIMIT = 4;
    private const int NORMAL_RANK_LIMIT = 8;

    [SerializeField] public bool hasCheatDeck = false;
    [SerializeField] public List<Card> deck = new();
    [SerializeField] public List<Card> cheatDeck = new();

    public void Startup(bool lake)
    {
        //switch (GameManager.instance.difficulty)
        //{
        //    case Difficulty.Easy:
        //        deck = BuildDeck(EASY_RANK_LIMIT, false);
        //        if (hasCheatDeck) cheatDeck = BuildDeck(EASY_RANK_LIMIT, true);
        //        break;
        //    case Difficulty.Normal:
        //        deck = BuildDeck(NORMAL_RANK_LIMIT, false);
        //        if (hasCheatDeck) cheatDeck = BuildDeck(NORMAL_RANK_LIMIT, true);
        //        break;
        //    case Difficulty.Hard:
        //        deck = BuildDeck(false);
        //        if (hasCheatDeck) cheatDeck = BuildDeck(true);
        //        break;
        //}

        if (lake) { deck = BuildDeck(false); return; }
        //Make full deck to draw from
        int[] ranks = new int[14];
        List<Card> fullDeck = new List<Card>();
        fullDeck = BuildDeck(false);

        while (deck.Count < 7)
        {
                int index = Random.Range(0, fullDeck.Count);
                Card c = fullDeck[index];
                int cardIndex = (int)c.rank;
                //If less than 1 of one kind of rank pulled, add it to the deck
                if (ranks[cardIndex] < 1)
                {
                ranks[cardIndex]++;
                deck.Add(Card.Copy(c));
                }
                fullDeck.RemoveAt(index);
        }

        
        //if (hasCheatDeck) cheatDeck = BuildDeck(true);
    }

    public void AddCard(Card card, int i)
    {
        deck.Insert(i, card);
    }

    public Card RemoveRandomCard(bool fromCheatDeck)
    {
        if (!fromCheatDeck && (deck == null || deck.Count == 0))
        {
            //Error handling
        }
        if (fromCheatDeck && !hasCheatDeck)
        {
            //Error handling
        }
        if (hasCheatDeck && fromCheatDeck && (cheatDeck == null || cheatDeck.Count == 0))
        {
            //Error handling
        }

        int max = fromCheatDeck ? cheatDeck.Count : deck.Count;
        int index = Random.Range(0, max);
        if (fromCheatDeck)
        {
            Card ca = cheatDeck[index];
            cheatDeck.RemoveAt(index);
            return ca;
        }
        Card c = deck[index];
        deck.RemoveAt(index);
        return c;
    }

    public bool RemovePair(List<Card> deck, Card card1, Card card2)
    {
        if (card1.rank != card2.rank) return false;
        //Find and remove both cards

        for (int i = 0; i < deck.Count; i++ )
        {
            Card ca = deck[i];
            if ((ca.rank == card1.rank && ca.suit == card1.suit) || (ca.rank == card2.rank && ca.suit == card2.suit))
            {
                deck.RemoveAt(i);
                i--;
            }
        }

        return true;
    }

    //Finds and return the index of the first card in the hand matching the given rank, -1 if does not exist
    public int FindRank(List<Card> deck, CardRank rank)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i].rank == rank) return i;
        }
        return -1;
    }

    //Builds and returns a list of cards representing a full 52 card standard deck with the given cheatDeck specification
    private List<Card> BuildDeck(bool forCheatDeck)
    {
        List<Card> deck = new();
        //Loop through every suit
        for (int i = 1; i <= MAX_SUITS; i++)
        {
            CardSuit suit = (CardSuit)i;
            //Loop through ranks up to limit
            for (int j = 1; j <= MAX_RANKS; j++)
            {
                CardRank rank = (CardRank)j;
                deck.Add(new Card(suit, rank, forCheatDeck));
            }
        }
        return deck;
    }

    //Builds and returns a list of cards representing a deck of cards with all suits up to/including the given rank with the given cheatDeck specification
    private List<Card> BuildDeck(CardRank maxRank, bool forCheatDeck)
    {
        int rankLimit = Mathf.Min(MAX_RANKS, (int)maxRank);
        List<Card> deck = new();
        //Loop through every suit
        for (int i = 1; i <= MAX_SUITS; i++)
        {
            CardSuit suit = (CardSuit)i;
            //Loop through ranks up to limit
            for (int j = 1; j <= rankLimit; j++)
            {
                CardRank rank = (CardRank)j;
                deck.Add(new Card(suit, rank, forCheatDeck));
            }
        }
        return deck;
    }

    //Returns the index of first Null card it finds in the deck. -1 if no null card exists.
    //Useful to check if any null cards exist in deck to reduce errors
    private int GetNullCard(List<Card> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i].isNull) return i;
        }
        return -1;
    }
}
