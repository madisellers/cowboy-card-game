using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerHand : MonoBehaviour
{
    private const float MAX_HAND_ANGLE_DEG = 90;

    public List<GameObject> cardObjects;

    [Header("References")]
    [SerializeField] private GameObject ghostCardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject cardPrefab;

    private int ghostCardIndex;

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
        CardObject co = card.GetComponent<CardObject>();
        co.SetUp();
        UpdateCardPositions();
    }

    public void UpdateCardPositions()
    {
        float cardSpacing = 1f;
        float firstCardPosition = 0.5f - (cardObjects.Count - 1f) * cardSpacing * 2;
        Spline spline = splineContainer.Spline;
        for (int i = 0; i < cardObjects.Count; i++)
        {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);
            //cardObjects[i].transform.DOMove(splinePosition, 0.25f);
            //cardObjects[i].transform.DOLocalRotateQuaternion(RotationDriveMode, 0.25f);
            cardObjects[i].transform.position = splinePosition;
            //cardObjects[i].transform.rotation = Quaternion.
        }
    }

    private void CheckGhostCardPlacement()
    {
        //Check can have ghost card
        //Get mouse position
        //Check if resonable near card spline 

        //Compare mouse X to card placement
        float mouseX;
        int replaceIndex = FindGhostCardIndex(mouseX);
        if (replaceIndex == ghostCardIndex) return;

        //If current ghost card placement doesnt match where mouse is, delete it and create new one where mouse is
        DestroyGhostCard();
        AddGhostCard();
    }

    private void DestroyGhostCard()
    {
        Destroy(cardObjects[ghostCardIndex];
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
