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

    private int ghostCardIndex = -1;

    private bool evenNumObj => cardObjects.Count % 2 == 0 ? true : false;

    private void Update()
    {
        CheckGhostCardPlacement();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DrawCard();
        }
    }

    private void DrawCard()
    {
        GameObject card = Instantiate(cardPrefab);
        cardObjects.Add(card);
        CardObject co = card.GetComponent<CardObject>();
        co.SetUp(CardRank.Four, CardSuit.Hearts);
        UpdateCardPositions();
    }

    public void UpdateCardPositions()
    {
        float startZ = 0;

        float totalLength = splineContainer.CalculateLength(0);
        float spacing = totalLength / cardObjects.Count;
        spacing = spacing < MAX_CARD_SPACING ? spacing : MAX_CARD_SPACING;

        //int[] poses = new int[cardObjects.Count];
        if (cardObjects.Count % 2 == 0)
        {
            float middlePos = totalLength / 2;
            int numCardsOnSide = cardObjects.Count - (cardObjects.Count / 2) + 1;
            int multiplier = -numCardsOnSide;
            for (int i = 0; i < cardObjects.Count; i++)
            {
                float position = (middlePos + spacing * multiplier) / totalLength;
                Vector3 splinePosition = splineContainer.EvaluatePosition(position);
                splinePosition.z = startZ + 0.01f * i;
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
                splinePosition.z = startZ + 0.01f * i;
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
        //Check can have ghost card
        //Get mouse position
        //Check if resonable near card spline 

        //Compare mouse X to card placement
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Set the Z value to a desired distance from the camera (e.g., 10 units)
        // or set to Camera.main.nearClipPlane for a close point
        mouseScreenPosition.z = 10f;

        // Convert the screen position to world space
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        // For 2D games, you might want to force the Z to 0
        // worldPosition.z = 0f; 
        float mouseX = worldPosition.x;
        int replaceIndex = FindGhostCardIndex(mouseX);
        if (replaceIndex == ghostCardIndex) return;

        //If current ghost card placement doesnt match where mouse is, delete it and create new one where mouse is
        DestroyGhostCard();
        AddGhostCard(replaceIndex);
    }

    private void DestroyGhostCard()
    {
        if (cardObjects.Count == 0) return;
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
            //Skip the ghost card
            if (i == ghostCardIndex) continue;
            float cardX = cardObjects[i].transform.position.x;
            bool mouseAfterCard = mouseX > cardX;
            //Mouse after last card
            if (i == cardObjects.Count - 1 && mouseAfterCard)
            {
                return i;
            }
            //Check if mouse is after current card card
            if (mouseAfterCard) continue;

            //if mouse before card, return the current index (card to be pushed forward)
            return i;

        }

        //return first position by default
        return 0;
    }



}
