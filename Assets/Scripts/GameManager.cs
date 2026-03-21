using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int turnNumber;
    public bool playerTurn;
    public CowboyState leadState;
    public CowboyState oppositeState;
    public TurnPhase turnPhase;

    void Awake()
    {
        //Create instance
        if (instance == null) instance = this;
        else Destroy(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
