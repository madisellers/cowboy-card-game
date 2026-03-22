using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D;
using UnityEngine.XR;

public class PlayerHand : MonoBehaviour
{
    private const float MAX_HAND_ANGLE_DEG = 90;
    private const float MAX_CARD_SPACING = 1f;

    public List<GameObject> cardObjects = new();

    [Header("References")]
    [SerializeField] private GameObject goFishButton;
    [SerializeField] private GameObject playerGun;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Camera mainCamera;


    [Header("References - Assets")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject ghostCardPrefab;
    [SerializeField] private GameObject highlightCard1;
    [SerializeField] private GameObject highlightCard2;
    [SerializeField] private GameObject requestBubble;
    [SerializeField] private GameObject goFishBubble;

    public int ghostCardIndex = -1;

    private (int card1, int card2) selected = (-1, -1);
    private bool pairCard1Picked = false;

    private bool evenNumObj => cardObjects.Count % 2 == 0 ? true : false;
    private GameManager gm => GameManager.instance;
    private CardHand hand => GetComponent<CardHand>();

    private void Start()
    {
        hand.Startup(false);
        for (int i = 0; i < hand.deck.Count; i++)
        {
            Card c = hand.deck[i];
            AddCard(c.rank, c.suit, i);
        }
    }

    private void Update()
    {
        //CheckGhostCardPlacement();
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    AddCard(CardRank.King, CardSuit.Spades, ghostCardIndex);
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    DrawCard();
        //}

        HandleControl();
    }

    private void HandleControl()
    {
        switch (gm.turnPhase)
        {
            case TurnPhase.RequestCard:
                if (gm.playerTurn)
                {
                    HandleGunActivation();
                    bool shot = HandleGunControl();
                    if (!playerGun.activeInHierarchy && !shot) RequestCard();
                }
                break;
            case TurnPhase.AnswerRequest:
                if (!gm.playerTurn)
                {
                    HandleGunActivation();
                    bool shot = HandleGunControl();
                    if (!playerGun.activeInHierarchy && !shot) AnswerRequest();
                }
                break;
            case TurnPhase.TakeCard:
                if (gm.playerTurn)
                {
                    HandleGunActivation();
                    bool shot = HandleGunControl();
                    if (!playerGun.activeInHierarchy && !shot) TakeCard();
                }
                break;
            case TurnPhase.PlacePair:
                if (gm.playerTurn)
                {
                    PlacePair();
                }
                break;
            default:
                break;
        }
    }

    private void RequestCard()
    {
        HighlightCard1();

        //If mouse clicked
        if (Input.GetMouseButtonDown(0))
        {
            //No card selected skip
            if (selected.card1 < 0) return;
            //Request selected card
            Card card = Card.Copy(hand.deck[selected.card1]);
            GameManager.instance.request = card;
            CheatManager.instance.SetCurrentTurnRequestRank(card.rank);
            DisplayCardRequest(card);
            selected.card1 = -1;
            GameManager.instance.ChangeTurnPhase(TurnPhase.AnswerRequest);
        }
    }

    private void AnswerRequest()
    {
        goFishButton.SetActive(true);
        HighlightCard1();
        //If no button click, skip
        if (!Input.GetMouseButtonDown(0)) return;

        
        Vector3 mousePos = MousePos();
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
        //Nothing found
        if (hitCollider == null) return;
        //Go fish button clicked
        if (hitCollider.gameObject == goFishButton)
        {
            //Find out if lying
            int index = hand.FindRank(hand.deck, gm.request.rank);
            //Lying
            if (index >= 0)
            {
                DisplayGoFish();
                GameManager.instance.GoFish();
                CheatManager.instance.SetCurrentTurnCardGiven(false);
                CheatManager.instance.SetCurrentTurnRequestInOppositeHand(true);
                GameManager.instance.ChangeTurnPhase(TurnPhase.TakeCard);
                goFishButton.SetActive(false);
                return;
            }
            //Truthful
            else
            {
                DisplayGoFish();
                GameManager.instance.GoFish();
                CheatManager.instance.SetCurrentTurnCardGiven(false);
                CheatManager.instance.SetCurrentTurnRequestInOppositeHand(false);
                goFishButton.SetActive(false);
                GameManager.instance.ChangeTurnPhase(TurnPhase.TakeCard);
                return;
            }
        }

        //Card clicked
        CardObject co = hitCollider.gameObject.GetComponent<CardObject>();
        if (co != null)
        {
            //skip if selected not set
            if (selected.card1 < 0) return;

            //Remove card from deck(s) and give to manager
            Card card = Card.Copy(hand.deck[selected.card1]);
            hand.deck.RemoveAt(selected.card1);
            RemoveCard(selected.card1);
            GameManager.instance.GiveCard(card);
            CheatManager.instance.SetCurrentTurnCardGiven(true);
            CheatManager.instance.SetCurrentTurnRequestInOppositeHand(true);
            goFishButton.SetActive(false);
            GameManager.instance.ChangeTurnPhase(TurnPhase.TakeCard);
        }

    }

    private void TakeCard()
    {
        //Highlight possible positions to place card in
        CheckGhostCardPlacement();

        //If no button click, skip
        if (!Input.GetMouseButtonDown(0)) return;

        //Add request asnwer to deck(s)
        int i = ghostCardIndex;
        hand.deck.Insert(i, Card.Copy(gm.requestAnswer));
        AddCard(gm.requestAnswer.rank, gm.requestAnswer.suit, i);

        //Reset ghost card stuff
        DestroyGhostCard();
        UpdateCardPositions();

        //Handle game/cheat manager stuff
        CheatManager.instance.SetCurrentTurnAddedCardIndex(i);
        GameManager.instance.ChangeTurnPhase(TurnPhase.PlacePair);

    }

    private void PlacePair()
    {
        //Check if there is a valid pair
        bool havePair = false;
        int[] ranks = new int[14];
        for (int i = 0; i < hand.deck.Count; i++)
        {
            int cardIndex = (int)hand.deck[i].rank;
            ranks[cardIndex]++;
            if (ranks[cardIndex] == 2)
            {
                havePair = true;
            }
        }
        //No pair in hand
        if (!havePair)
        {
            CheatManager.instance.SetCurrentTurnPairDroppedIndeces(-1, -1);
            GameManager.instance.ChangeTurnPhase(TurnPhase.RequestCard);
        }

        //If card 1 already selected
        if (selected.card1 >= 0 && pairCard1Picked)
        {
            HighlightCard2();

            //If no button click, skip
            if (!Input.GetMouseButtonDown(0)) return;

            Vector3 mousePos = MousePos();
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
            //Nothing found
            if (hitCollider == null) return;
            //Card clicked
            CardObject co = hitCollider.gameObject.GetComponent<CardObject>();
            if (co != null)
            {
                //skip if selected not set
                if (selected.card1 < 0) return;

                //If highlight card 1 clicked, undo pair
                if (selected.card2 == selected.card1)
                {
                    pairCard1Picked = false;
                    selected.card2 = -1;
                    return;
                }

                //if second card clicked does not match in rank, skip
                CardRank r1 = hand.deck[selected.card1].rank;
                CardRank r2 = hand.deck[selected.card2].rank;
                if (r1 != r2) return;

                //Remove cards from deck(s)
                int first = selected.card1 < selected.card2 ? selected.card1 : selected.card2;
                int second = first == selected.card1 ? selected.card2 : selected.card2;
                Card card1 = Card.Copy(hand.deck[first]);
                Card card2 = Card.Copy(hand.deck[second]);
                RemoveCard(second);
                RemoveCard(first);
                hand.RemovePair(hand.deck, card2, card1);

                //Update managers
                CheatManager.instance.SetCurrentTurnPairDroppedIndeces(first, second);
                gm.playerPairs++;
                gm.ChangeTurnPhase(TurnPhase.RequestCard);
            }

        }
        //Card 1 needs to be selected
        else
        {
            //Select Card 1
            HighlightCard1();
            //If no button click, skip
            if (!Input.GetMouseButtonDown(0)) return;


            Vector3 mousePos = MousePos();
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
            //Nothing found
            if (hitCollider == null) return;

            //Card clicked
            CardObject co = hitCollider.gameObject.GetComponent<CardObject>();
            if (co != null)
            {
                //skip if selected not set
                if (selected.card1 < 0) return;

                //Mark pair card 1 as being picked
                pairCard1Picked = true;
            }
        }
    }

    private void DrawCard()
    {
        GameObject card = Instantiate(cardPrefab);
        cardObjects.Add(card);
        CardObject co = card.GetComponent<CardObject>();
        co.SetUp(CardRank.Queen, CardSuit.Hearts);
        UpdateCardPositions();
    }

    private void AddCard(CardRank rank, CardSuit suit, int index)
    {
        GameObject card = Instantiate(cardPrefab);
        
        CardObject co = card.GetComponent<CardObject>();
        co.SetUp(rank, suit);
        if (index == ghostCardIndex)
        {
            DestroyGhostCard();
            if (index > cardObjects.Count)
            {
                cardObjects.Add(card);
            }
            else
            {
                cardObjects.Insert(index, card);
            }
        }
        else
        {
            cardObjects.Insert(index, card);
            ghostCardIndex = ghostCardIndex > index ? ghostCardIndex + 1 : ghostCardIndex;
        }
            
        UpdateCardPositions();
    }

    public void UpdateCardPositions()
    {
        float startZ = 0;

        float totalLength = splineContainer.CalculateLength(0);
        float spacing = totalLength / cardObjects.Count;
        spacing = spacing < MAX_CARD_SPACING ? spacing : MAX_CARD_SPACING;

        //int[] poses = new int[cardObjects.Count];
        if (cardObjects.Count % 2 == 1)
        {
            float middlePos = totalLength / 2;
            int numCardsOnSide = cardObjects.Count - ((cardObjects.Count / 2) + 1);
            int multiplier = -numCardsOnSide;
            for (int i = 0; i < cardObjects.Count; i++)
            {
                float position = (middlePos + spacing * multiplier) / totalLength;
                Vector3 splinePosition = splineContainer.EvaluatePosition(position);
                splinePosition.z = startZ - 0.01f * i;
                Vector3 forward = splineContainer.EvaluateTangent(position);
                Vector3 up = splineContainer.EvaluateUpVector(position);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                //Set up the card
                cardObjects[i].transform.position = splinePosition;

                //Update values
                multiplier++;

            }
        }
        else
        {
            float middlePos = (totalLength / 2) - (spacing / 2);
            int numCardsOnSide = cardObjects.Count / 2;
            int multiplier = -numCardsOnSide;

            for (int i = 0; i < cardObjects.Count; i++)
            {
                float position = (middlePos + spacing * multiplier) / totalLength;
                Vector3 splinePosition = splineContainer.EvaluatePosition(position);
                splinePosition.z = startZ - 0.01f * i;
                Vector3 forward = splineContainer.EvaluateTangent(position);
                Vector3 up = splineContainer.EvaluateUpVector(position);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                //Set up the card
                cardObjects[i].transform.position = splinePosition;

                //Update values
                multiplier++;

            }
        }
    }

    private void CheckGhostCardPlacement()
    {
        ////Compare mouse X to card placement
        //Vector3 mouseScreenPosition = Input.mousePosition;

        //// Set the Z value to a desired distance from the camera (e.g., 10 units)
        //// or set to Camera.main.nearClipPlane for a close point
        //mouseScreenPosition.z = 10f;

        //// Convert the screen position to world space
        //Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        //// For 2D games, you might want to force the Z to 0
        //worldPosition.z = 0f; 
        Vector3 mousePos = MousePos();
        float mouseX = mousePos.x;
        int replaceIndex = FindGhostCardIndex(mouseX);
        Debug.Log(mouseX);
        //Debug.Log(replaceIndex);
        if (replaceIndex == ghostCardIndex) return;

        //If current ghost card placement doesnt match where mouse is, delete it and create new one where mouse is
        Debug.Log("Making new ghost card at i == " + replaceIndex);
        DestroyGhostCard();
        AddGhostCard(replaceIndex);
        UpdateCardPositions();
    }

    private void DestroyGhostCard()
    {
        if (cardObjects.Count == 0) return;
        if (ghostCardIndex < 0) return;
        Destroy(cardObjects[ghostCardIndex]);
        cardObjects.RemoveAt(ghostCardIndex);
        ghostCardIndex = -1;
    }

    private void AddGhostCard(int index)
    {
        GameObject go = Instantiate(ghostCardPrefab);
        cardObjects.Insert(index, go);
        ghostCardIndex = index;
    }


    private int FindGhostCardIndex(float mouseX)
    {
        int mouseCardIndex = 0;
        for (int i = 0; i < cardObjects.Count; i++)
        {
            float cardX = cardObjects[i].transform.position.x;
            bool mouseAfterCard = mouseX > cardX;
            //Mouse after last card
            if (i == cardObjects.Count - 1 && mouseAfterCard)
            {
                Debug.Log("Mouse after last card.");
                return i;
            }
            //Check if mouse is after current card card
            if (mouseAfterCard) continue;

            //if mouse before card, return the current index (card to be pushed forward)
            Debug.Log("Mouse before card. i == " + i);

            return i;

        }

        //return first position by default
        return 0;
    }

    private Vector3 MousePos()
    {
        //Compare mouse X to card placement
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Set the Z value to a desired distance from the camera (e.g., 10 units)
        // or set to Camera.main.nearClipPlane for a close point
        mouseScreenPosition.z = 10f;

        // Convert the screen position to world space
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        // For 2D games, you might want to force the Z to 0
        worldPosition.z = 0f;
        return worldPosition;
    }

    private int FindCardObject(GameObject co)
    {
        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (co == cardObjects[i])
            {
                return i;
            }
        }
        return -1;
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

    //Updates selectable card 1 and places a highlight over it
    private void HighlightCard1()
    {
        Vector3 mousePos = MousePos();
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
        if (hitCollider == null) { selected.card1 = -1; highlightCard1.SetActive(false); return; }
        CardObject co = hitCollider.gameObject.GetComponent<CardObject>();
        if (co == null) { selected.card1 = -1; highlightCard1.SetActive(false); return; }
        //If selectable card not found yet
        if (selected.card1 < 0)
        {
            //Find index of object it corresponds to adn save it
            int ind = FindCardObject(co.gameObject);
            if (ind == -1) return;
            selected.card1 = ind;
            //Place highlight
            highlightCard1.SetActive(true);
            Vector3 pos = co.transform.position;
            pos.z -= 5f;
            highlightCard1.transform.position = pos;
        }
    }

    //Updates selectable card 1 and places a highlight over it
    private void HighlightCard2()
    {
        Vector3 mousePos = MousePos();
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
        if (hitCollider == null) { selected.card2 = -1; highlightCard2.SetActive(false); return; }
        CardObject co = hitCollider.gameObject.GetComponent<CardObject>();
        if (co == null) { selected.card2 = -1; highlightCard2.SetActive(false); return; }
        //If selectable card not found yet
        if (selected.card2 < 0)
        {
            //Find index of object it corresponds to adn save it
            int ind = FindCardObject(co.gameObject);
            if (ind == -1) return;
            if (ind == selected.card1) { selected.card2 = ind; highlightCard2.SetActive(false); return; }
            selected.card2 = ind;
            //Place highlight
            highlightCard2.SetActive(true);
            Vector3 pos = co.transform.position;
            pos.z -= 5f;
            highlightCard2.transform.position = pos;
        }
    }

    private void RemoveCard(int i)
    {
        GameObject go = cardObjects[i];
        cardObjects.RemoveAt(i);
        Destroy(go);
    }

    private bool HandleGunControl()
    {
        if (!playerGun.activeInHierarchy) return false;

        if (Input.GetMouseButtonDown(0))
        {
            GameManager.instance.GameOver(false);
            return true;
        }
        return false;
    }

    private void HandleGunActivation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            playerGun.SetActive(true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            playerGun.SetActive(false);
        }
    }

}
