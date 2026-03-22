using UnityEngine.Rendering.Universal.Internal;

public enum CardSuit
{
    NULL,
    Hearts,
    Diamonds,
    Spades,
    Clubs
}

public enum CardRank
{
    NULL,
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King
}

public enum CheatType
{
    NULL,
    AddCard,
    WithheldWhenAsked
}

public struct Card
{
    public CardSuit suit;           //This cards suit
    public CardRank rank;           //This cards rank
    public bool fromCheatDeck;      //True if this card was added from the cowboy's OWN cheat deck, false otherwise. If given to lead, auto reset to false (kind of like money laundering)

    public bool isNull => (suit == CardSuit.NULL || rank == CardRank.NULL);	//Check if card is null, easy to check for errors


    public Card(CardSuit suit, CardRank rank, bool fromCheatDeck)
    {
        this.suit = suit;
        this.rank = rank;
        this.fromCheatDeck = fromCheatDeck;
    }

    public static Card Copy(Card card)
    {
        Card c = new Card(card.suit, card.rank, card.fromCheatDeck);
        return c;
    }

    public string ToString()
    {
        return $"({rank} {suit}" + (fromCheatDeck ? "CHEAT" : "") + ")";
    }
}

public enum TurnPhase
{
    RequestCard,
    AnswerRequest,
    TakeCard,
    PlacePair
}

/*
 * TURN PHASE LOOP + WHAT COWBOYS CAN DO DURING EACH (Turn lead (L) vs opposite (O))
 * ---------------------------------------------------
 * RequestCard
 * 		-L: Select card and ask opposite for selected rank, add cheat card
 * 		-O: Add cheat card
 * AnswerRequest
 *		-L: Add cheat card
 *		-O: Select request answer, add cheat card
 * TakeCard:
 *		-L: Take opposite's given card or go fish
 *		-O: Add cheat card
 * PlacePair:
 *		-L: Select two cards to place down as pair or select end turn
 *		-O: Add cheat card
 */

enum Difficulty
{
    Easy,
    Normal,
    Hard
}



