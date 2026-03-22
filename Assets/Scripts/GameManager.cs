using System.Collections;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int turnNumber;
    public bool playerTurn;
    public CowboyState leadState;
    public CowboyState oppositeState;
    public TurnPhase turnPhase;
    public Card request;
    public Card requestAnswer;

    //References
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject npc;
    [SerializeField] private CardHand lake;
    [SerializeField] private GameObject npcShoots;
    [SerializeField] private GameObject playerShoots;
    [SerializeField] private GameObject gunSmoke;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject winScreen;

    private NPC Npc => npc.GetComponent<NPC>();
    private PlayerHand playerHand => player.GetComponent<PlayerHand>();
    private CheatManager cm => CheatManager.instance;

    public int npcPairs = 0;
    public int playerPairs = 0;
    public bool wentFishing = false;

    private bool phaseChanged = true;

    void Awake()
    {
        //Create instance
        if (instance == null) instance = this;
        else Destroy(gameObject);

        lake.Startup(true);

    }

    private void Start()
    {
        turnNumber = 1;
        playerTurn = false;
        SwitchLeadOppositeStates();
    }

    private void Update()
    {
        if (phaseChanged)
        {
            phaseChanged = false;
            HandleTurnPhaseChange();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }
    }

    public void ChangeTurnPhase(TurnPhase turnPhase)
    {
        if (this.turnPhase == TurnPhase.PlacePair && turnPhase == TurnPhase.RequestCard) EndTurn();

        this.turnPhase = turnPhase;
        phaseChanged = true;

        //Update any managers
        CheatManager.instance.UpdateIsLying();
    }

    public void HandleTurnPhaseChange()
    {
        if (playerTurn)
        {
            switch (turnPhase)
            {
                case TurnPhase.RequestCard:
                    break;
                case TurnPhase.AnswerRequest:
                    Npc.AnswerRequest(request.rank);
                    break;
                case TurnPhase.TakeCard:
                    break;
                case TurnPhase.PlacePair:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (turnPhase)
            {
                case TurnPhase.RequestCard:
                    Npc.RequestCard();
                    break;
                case TurnPhase.AnswerRequest:
                    break;
                case TurnPhase.TakeCard:
                    Npc.TakeCard();
                    break;
                case TurnPhase.PlacePair:
                    Npc.PlacePair();
                    break;
                default:
                    break;
            }
        }
    }

    public void GoFish()
    {
        Card c = lake.RemoveRandomCard(false);
        requestAnswer = c;
        wentFishing = true;
    }

    public void GiveCard(Card card)
    {
        requestAnswer = card;
        wentFishing = false;
    }

    public void EndTurn()
    {
        CheatManager.instance.AddTurnToHistory();
        turnPhase = TurnPhase.RequestCard;
        turnNumber++;
        playerTurn = !playerTurn;
        SwitchLeadOppositeStates();
        cm.SetCurrentTurnStartInfo(turnNumber, playerTurn);
    }

    public void GameOver(bool npcShot, bool playerLead)
    {
        if (npcShot)
        {
            StartCoroutine(NPCShoots());
            return;
        }

        //Player shoots
        //Check if valid shot
        if ((playerLead && CheatManager.instance.canLeadShoot) || (!playerLead && CheatManager.instance.canOppositeShoot))
        {
            StartCoroutine(PlayerShootsCorrect());
        }
        else
        {
            StartCoroutine(PlayerShootsIncorrect());
        }
    }

    private void SwitchLeadOppositeStates()
    {
        if (playerTurn)
        {
            leadState = player.GetComponent<CowboyState>();
            oppositeState = npc.GetComponent<CowboyState>();
        }
        else
        {
            leadState = npc.GetComponent<CowboyState>();
            oppositeState = player.GetComponent<CowboyState>();
        }
    }

    IEnumerator NPCShoots()
    {
        yield return new WaitForSeconds(3f);
        npcShoots.SetActive(true);
        yield return new WaitForSeconds(1f);
        loseScreen.SetActive(true);
        yield return null;
    }

    IEnumerator PlayerShootsCorrect()
    {
        playerShoots.SetActive(true);
        yield return new WaitForSeconds(2f);
        winScreen.SetActive(true);
        yield return null;
    }

    IEnumerator PlayerShootsIncorrect()
    {
        gunSmoke.SetActive(true);
        yield return new WaitForSeconds(3f);
        npcShoots.SetActive(true);
        yield return new WaitForSeconds(1f);

        loseScreen.SetActive(true);

        yield return null;
    }
}
