using System.Collections.Generic;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    /*
 * RULES OF GAME RELEVANT TO CHEATING POTENTIALS
 * ------------------------------------------------
 * - If a cowboy obtains a pair by being given a card, he must place some combo of pair on the table, regardless of either cards cheat status.
 * 		-Of course, if this given card is placed in index and pair chosen from different indeces, this results in lying
 * - If a cowboy obtains a pair by going fishing, he must place it on the table UNLESS on of the pair cards was draw via cheating.
 * - If a cowboy has a cheat pair not yet placed, and obtains the same rank in the pair via going fishing, he MUST place this new valid pair on the table
 * 		- A cheat pair can only be placed after a Go Fish, if no valid pair is present
 * - Cowboys can only request a rank that is already in their hand
 * - Pairs placed on table are still kept secret until game over.
 * - When first handed cards at start of game, there will NEVER be initial pairs. All cards will have unique ranks for both cowboys in initial draw.
 * 
 * WHEN DOES CHEATING COUNT AS CHEATING
 * ------------------------------------------------
 * - Noticing an extra card in the hand is not enough to prove cheating, catch must be logic-based or in the middle of action
 * - Can catch adding an extra card only if the cowboy is in the middle of the action. Once card in deck, no longer cheating.
 * 		- Similarly, if player pulls gun during adding card, card auto added to hand, NPC state cuts to scared. If gun put away,
 *		  action is over, and no longer considered cheating.
 * - A lie can only be caught at the first phase that it can be logically deduced, not after. If you realize a lie at any point
 *   after, including a phase change, you cannot act on it validly.
 * 
 * 
 * 
 * 
 * 
 * 
 * POTENTIAL SCENARIOs (Second cowboy is potential cheater in these scenarios)
 * --------------------------------------------------
 * A) First cowboy asks for 2s. Second cowboy says no (either). First cowboy goes fishing. Second cowboy asks for 2s. First cowboy CAN shoot. (Either drew cheat card or lied about having it before)
 * B) First cowboy asks for 2s. Second cowboy says no (lie). First cowboy goes fishing. Second cowboy asks for 4s. First cowboy says yes, gives card. 
 *   Second cowboy places it in index 5. Takes pair cards from index 2 and 4. First CAN shoot.
 * C) First cowboy asks for 2s. Second cowboy says no (lie). First cowboy goes fishing. Second cowboy adds cheated 2 card successfully. Second cowboy asks for 4s. 
 *   First cowboy says no (either). Second cowboy goes fishing. Second cowboy places it in index 5. Takes pair cards from index 2 and 4. First CAN shoot.
 * D) First cowboy asks for 2s. Second says no (lie). First cowboy goes fishing. Second cowboy adds cheated 2 card successfully. Second cowboy asks for 4s. 
 *   First says says no (either). Second goes fishing, places it in index 5. Takes pair cards from index 5 and 4. First CANNOT shoot. Still could be legit pair, not guarunteed cheating.
 * 
 * E) First cowboy asks for 2s, Second says no (truth). First goes fishing. Second asks for 4s. First gives a 4. Second makes pair with it and drops it. First asks for 2s. 
 *  Second sucessfully adds a cheat 2. Second says yes. First CAN shoot. Since Second didn't go fish and card given didn't match the card given, the only way they could have 
 *  gotten a 2 was to add it from cheat pile
 * F) First cowboy asks for 2s, Second says no (lie). First goes fishing. Second asks for 4s. First gives a 4. Second makes pair with it and drops it. First asks for 2s. 
 *  Second says yes. First CAN shoot. Since Second didn't go fish and card given didn't match previous, the only way they could have had a 2 was if they lied about having it 2 turns ago.
 * 
 * WHEN CAN PEOPLE LIE? aka when updates are run at start of turn phase, can it be the first catch of a lie (In terms of lead and opposite)
 * ----------------------------------------------------------------------
 * RequestCard
 * 		-L: No
 * 		-O: Yes (Scenario B, C)
 * AnswerRequest
 *		-L: Yes (scneario A)
 *		-O: No
 * TakeCard:
 *		-L: No
 *		-O: Yes (Scenario E,F)
 * PlacePair:
 *		-L: No
 *		-O: No
 * 
 * WHEN CAN PEOPLE SHOOT IN CASE OF A LIE? (Not necesarily valid, but can attempt)
 * ----------------------------------------------------------------------
 * RequestCard
 * 		-L: Yes (valid if notice O's lie from pair placement, see scenario B, C)
 * 		-O: No
 * AnswerRequest
 *		-L: No
 *		-O: Yes (valid if request clashes with previous turns response, i.e. said no to having requested and requested same as previous without putting any pairs down, see scenario A)
 * TakeCard:
 *		-L: Yes (valid rare, If O previously denied request, didn't go fish last turn, if given card for pair and didn't match request last turn, and same request this turn, but now O has it,
 *				the only way O could have gotten it was through cheating or by lying the first time, See scenario E or F)
 *		-O: No
 * PlacePair:
 *		-L: No
 *		-O: No
 * 
 * FAQ
 * -------------------------------------------------------------------------------------
 * Q) What's the benefit to adding cards if they count against me if not paired in the end?
 * Added cards can still paired, with either valid cards or your added cards, adding to your total pair count in the end. Even if they are put on the
 * table before the game is declared over, they will still be counted as pairs when auto counting the remaining cards.
 * You can give cheated cards to your opponent if asked and they will be none the wiser. If you give them enough and bank on them 
 * not having the appropriate card to pair it with in the end (pretty good chance of this happening, about 49/52 chance across 3 full decks), you can give them more
 * points against them instead?
 * 
 * Q) So is it better to just add a bunch of cheated cards when I can?
 * Well, you still have to get away with it and this changes your total card count. While your opponent can't shoot you just based on card count,
 * it can raise their suspicion the more cards you've added that logically can't be there, making it more likely they will catch you in a lie.
 * 
 * 
 * 
 * 
 */

    //Keeps track of what occured on a turn
    public struct Turn
    {
        public int number;                                      //The turn number
        public bool playerIsLead;                               //True if this is player's turn, false if NPCs (Player is always odd turn)
        public CardRank requestRank;                            //The rank of the request
        public bool requestInOppositeHand;                      //Is the requested rank in the opposite's hand?
        public bool cardGiven;                                  //Was the requested rank given to turn lead?
        public int addedCardIndex;                              //The placement index of the card that was added to the lead's deck (either taken or fished)
        public (int card1, int card2) pairDropped;              //The indeces of the cars that were pair dropped, -1 if no pair dropped

        //public int leadDeckCount;								//How many cards were in the deck of the lead's hand by the end of the turn
        //public int oppositeDeckCount;							//How many cards were in the deck of the opposite's hand by the end of the turn
        public bool addedCardInPairDropped => addedCardIndex == pairDropped.card1 || addedCardIndex == pairDropped.card1;
        public bool oppositeLied => requestInOppositeHand && !cardGiven;

        public bool isNull => requestRank == CardRank.NULL;

        public static Turn Null()
        {
            Turn t = new();
            t.requestRank = CardRank.NULL;
            return t;
        }
    }

    //Constants
    private const int TURNS_HISTORY_COUNT_MAX = 3;

    //Class instance
    public static CheatManager instance;

    //Public attributes
    public bool canLeadShoot => oppositeIsLying || oppositeIsCheating;          //Can the turn lead correctly shoot the opposite for cheating
    public bool canOppositeShoot => leadIsLying || leadIsCheating;              //Can the turn opposite correctly shoot the lead for cheating

    //References
    private int turnNumber => GameManager.instance.turnNumber;
    private bool playerTurn => GameManager.instance.playerTurn;
    private CowboyState leadState => GameManager.instance.leadState;
    private CowboyState oppositeState => GameManager.instance.oppositeState;
    private TurnPhase currentPhase => GameManager.instance.turnPhase;

    //Attributes
    private Turn currentTurn;
    private LinkedList<Turn> turnHistory;                                   //Keeps a running record of the turns information, where the first node is the latest and the last node is the oldest
    private bool oppositeIsLying;
    private bool oppositeIsCheating;
    private bool leadIsLying;
    private bool leadIsCheating;

    //Useful lambdas
    private Turn PrevTurn => turnHistory.First.Value;                       //Get the previous turn
    //private Turn LastLeadMatchTurn => PrevTurn.Next.Value;                  //Get the first turn whose lead matches the current turn's lead (aka previous previous turn)

    //Fun end game cheat stats
    private int opponentAddedCheatCardsCount = 0;
    private int opponentLiedInAnswerCount = 0;
    private int playerAddedCheatCardsCount = 0;
    private int playerLiedInAnswerCount = 0;

    void Awake()
    {
        //Create static instance here
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void AddTurnToHistory()
    {
        if (currentTurn.isNull)
        {
            Debug.LogError("CheatManager's currentTurn is null!");
            Debug.Break();
            return;
        }

        turnHistory.AddFirst(currentTurn);
        if (turnHistory.Count > TURNS_HISTORY_COUNT_MAX)
        {
            turnHistory.RemoveLast();
        }

    }

    public void UpdateIsLying()
    {
        UpdateOppositeIsLying();
        UpdateLeadIsLying();
    }

    //Tircky becaus eneed to keep track of whole segment + opposite/lead flips
    //Called at very beginning of every turn phase change
    private void UpdateOppositeIsLying()
    {
        //Scenario B, C
        if (currentPhase == TurnPhase.RequestCard)
        {
            //If opposite (who was lead last turn) dropped a pair in which niether were from the added index
            if (!PrevTurn.addedCardInPairDropped)
            {
                oppositeIsLying = true;
                return;
            }
        }

        //Scenario E, F
        //CHeck more than 1 turn occured
        if (turnHistory.Count < 2) return;

        //The only phase the opposite can lie in is the AnswerRequest, so only have to update after that
        if (currentPhase != TurnPhase.TakeCard) return;

        //Current phase is TakeCard (the opposite might have lied in previous phase)
        Turn firstDenial = new Turn();// = LastLeadMatchTurn;

        //If card given, no lie possible
        if (firstDenial.cardGiven) return;

        //Check previous turn, the opp now was the lead then and vice versa
        //Card was given matching request and pair was made with it
        if (PrevTurn.cardGiven && PrevTurn.addedCardInPairDropped)
        {
            //This turns request matches the first denial request
            if (firstDenial.requestRank == currentTurn.requestRank)
            {
                //Opposite tells truth
                if (!currentTurn.oppositeLied)
                {
                    oppositeIsLying = true;
                    return;
                }
            }

        }

        //Opposite not lying
        oppositeIsLying = false;
    }

    //Tircky becaus eneed to keep track of whole segment + opposite/lead flips
    //Called at very beginning of every turn phase change
    private void UpdateLeadIsLying()
    {
        //How to do it:
        //For every phase, check for all possible lies
        //Scenario A
        if (currentPhase == TurnPhase.AnswerRequest)
        {
            //If opposite lied last turn and request rank is same as last turn
            if (PrevTurn.oppositeLied && currentTurn.requestRank == PrevTurn.requestRank)
            {
                leadIsLying = true;
                return;
            }
        }

        //Lead not lying
        leadIsLying = false;
    }

    //When called when bringing gun up, call this first before updating anything else!
    //Then when called when putting gun down, call this last
    //In case of NPC AI, figure out when to call. NEED TO CALL EVERYTIMe, INCLUDING TO RESET AS NOT CHEATING
    private void UpdateOppositeIsCheating()
    {
        //if (leadState.canSeeOpponent && oppositeState.isCheating)
        //{
        //    oppositeIsCheating = true;
        //}
        //else
        //{
        //    oppositeIsCheating = false;
        //}
    }

    //When called when bringing gun up, call this first before updating anything else!
    //Then when called when putting gun down, call this last
    //In case of NPC AI, figure out when to call. NEED TO CALL EVERYTIMe, INCLUDING TO RESET AS NOT CHEATING
    private void UpdateLeadIsCheating()
    {
        //if (oppositeState.canSeeOpponent && leadState.isCheating)
        //{
        //    leadIsCheating = true;
        //}
        //else
        //{
        //    leadIsCheating = false;
        //}
    }

    //-------------------Update currentTurnInfo-----------------------------------
    public void SetCurrentTurnRequestRank(CardRank rank)
    {
        currentTurn.requestRank = rank;
    }

    public void SetCurrentTurnRequestInOppositeHand(bool b)
    {
        currentTurn.requestInOppositeHand = b;
    }

    public void SetCurrentTurnCardGiven(bool b)
    {
        currentTurn.cardGiven = b;
    }

    public void SetCurrentTurnAddedCardIndex(int i)
    {
        currentTurn.addedCardIndex = i;
    }

    //---------------------Debug methods (DO NOT USE FOR GAME PURPOSES-------------------------------
    public Turn GetPrevTurn()
    {
        return PrevTurn;
    }
}
