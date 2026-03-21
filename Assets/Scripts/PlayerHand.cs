using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D;

public class PlayerHand : MonoBehaviour
{
    private const float MAX_HAND_ANGLE_DEG = 90;
    private const float MAX_CARD_SPACING = 1f;

    public List<GameObject> cardObjects;

    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject ghostCardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Camera mainCamera;

    public int ghostCardIndex = -1;

    private bool evenNumObj => cardObjects.Count % 2 == 0 ? true : false;

    private void Update()
    {
        CheckGhostCardPlacement();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddCard(CardRank.King, CardSuit.Spades, ghostCardIndex);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            DrawCard();
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
        //Compare mouse X to card placement
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Set the Z value to a desired distance from the camera (e.g., 10 units)
        // or set to Camera.main.nearClipPlane for a close point
        mouseScreenPosition.z = 10f;

        // Convert the screen position to world space
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        // For 2D games, you might want to force the Z to 0
        worldPosition.z = 0f; 
        float mouseX = worldPosition.x;
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



}
