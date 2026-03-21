using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerHand : MonoBehaviour
{
    private const float MAX_HAND_ANGLE_DEG = 90;

    public List<GameObject> cardObjects;

    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject ghostCardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Camera mainCamera;

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
        GameObject card = Instantiate(cardPrefab, splineContainer.gameObject.transform);
        cardObjects.Add(card);
        CardObject co = card.GetComponent<CardObject>();
        co.SetUp(CardRank.Four, CardSuit.Hearts);
        UpdateCardPositions();
    }

    public void UpdateCardPositions()
    {
        float cardSpacing = 1f;
        float firstCardPosition = 0.5f - (cardObjects.Count - 1f) * cardSpacing * 2;
        Spline spline = splineContainer.Spline;
        float startZ = 0;
        for (int i = 0; i < cardObjects.Count; i++)
        {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);
            //cardObjects[i].transform.DOMove(splinePosition, 0.25f);
            //cardObjects[i].transform.DOLocalRotateQuaternion(RotationDriveMode, 0.25f);
            splinePosition.z = startZ + 0.01f * i;
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
        Destroy(cardObjects[ghostCardIndex]);
        ghostCardIndex = -1;
    }

    private void AddGhostCard(int index)
    {
        GameObject go = Instantiate(ghostCardPrefab, splineContainer.gameObject.transform);
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
