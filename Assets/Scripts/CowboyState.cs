using UnityEngine;

public enum CowboyStates
{
    //Player and NPC states
    LookAway,
    LookAtOpponent,
    CheckHand,
    LowerHand,
    GoFish,
    TakeOpponentCard,
    TakeCheatCard,
    PlacePair,

    //NPC only states
    OrderDrink,
    Drink,
    RevealHand,
    CatchCheat,
    Scared
}

//Can apply to both NPC opponent AND player
public class CowboyState : MonoBehaviour
{

    public CowboyStates currentState;

    //States
    private bool canSeeOpponent;
    private bool isCheating;
    private bool holdingGun;

    private CardRank lastRequestedRank;                 //What did they last ask for
    private int lastAddedCardIndex;                     //What index was the last card the opposing cowboy added to their hand? IMPORTANT
    private bool givenLastRequestedRank;                //Were they given their last requested rank


    void Update()
    {

    }


}
