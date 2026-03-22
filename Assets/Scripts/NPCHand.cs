using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class NPCHand : MonoBehaviour
{
    private const float MAX_HAND_ANGLE_DEG = 90;
    private const float MAX_CARD_SPACING = 1f;

    public List<GameObject> cardObjects;

    [Header("References")]
    [SerializeField] private GameObject backCardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Camera mainCamera;

    public int ghostCardIndex = -1;

    private bool evenNumObj => cardObjects.Count % 2 == 0 ? true : false;

    public void AddCard(int index)
    {
        GameObject card = Instantiate(backCardPrefab);
        cardObjects.Insert(index, card);
        Vector3 addedCardFinalPosition;
        UpdateCardPositions(index, out addedCardFinalPosition);
        StartCoroutine(CardSlideDown(index, addedCardFinalPosition));
    }

    public void RemoveCard(int index)
    {
        GameObject go = cardObjects[index];
        cardObjects.RemoveAt(index);
        StartCoroutine(CardSlideUp(index, true));
    }

    private void UpdateCardPositions(int addedCardIndex, out Vector3 addedCardFinalPosition)
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
                if (i == addedCardIndex) addedCardFinalPosition = splinePosition;
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
                if (i == addedCardIndex) addedCardFinalPosition = splinePosition;
                Vector3 forward = splineContainer.EvaluateTangent(position);
                Vector3 up = splineContainer.EvaluateUpVector(position);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                //Set up the card
                cardObjects[i].transform.position = splinePosition;

                //Update values
                multiplier++;

            }
        }

        addedCardFinalPosition = Vector3.zero;
    }

    System.Collections.IEnumerator CardSlideDown(int index, Vector3 finalPosition)
    {
        Vector3 newStartPos = cardObjects[index].transform.position;
        newStartPos.y += 3f;
        cardObjects[index].transform.position = newStartPos;
        bool closeToFinal = newStartPos.y < finalPosition.y;
        float MOVE_SPEED = 5f;
        while (!closeToFinal)
        {
            Vector3 newPos = cardObjects[index].transform.position;
            newPos.y -= MOVE_SPEED * Time.deltaTime;
            cardObjects[index].transform.position = newPos;
            closeToFinal = newStartPos.y < finalPosition.y;
            yield return null;
        }
        cardObjects[index].transform.position = finalPosition;
    }

    System.Collections.IEnumerator CardSlideUp(int index, bool destroy)
    {
        float startY = cardObjects[index].transform.position.y;
        float finalY = startY + 5f;
        Vector3 finalPosition = cardObjects[index].transform.position;
        finalPosition.y = finalY;
        bool closeToFinal = startY > finalY;
        float MOVE_SPEED = 5f;
        while (!closeToFinal)
        {
            Vector3 newPos = cardObjects[index].transform.position;
            newPos.y += MOVE_SPEED * Time.deltaTime;
            cardObjects[index].transform.position = newPos;
            closeToFinal = newPos.y > finalY;
            yield return null;
        }
        if (destroy)
        {
            Destroy(cardObjects[index]);
            cardObjects.RemoveAt(index);
            Vector3 addedCardFinalPosition;
            UpdateCardPositions(-1, out addedCardFinalPosition);
            yield return null;
        }
        cardObjects[index].transform.position = finalPosition;
    }
}
