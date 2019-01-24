using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    private Rigidbody2D myRigidbody2;
    [SerializeField] private float speed = 4f;
    private Vector2 nextDir;
    public Vector2 NextDir { get{return nextDir;} set{nextDir = value;}}
    private Vector2 dir;
    private bool firstTime1=true;
    private int Arrow=3; //0:UpArrow, 1: DownArrow, 2: LeftArrow, 3: RighArrow
    private int score = 0;
    [SerializeField] private int totalOfPacdots=4;
    public int TotalOfPacdots {get{return totalOfPacdots;} set {totalOfPacdots = value;}}
    [SerializeField] private int numOfPacdotsEaten=0;//pacdot comidos pelo player (+1 por cada pacdot comido independentemente do score ganhado) 
    public int NumOfPacdotsEaten {get {return numOfPacdotsEaten;}set {numOfPacdotsEaten = value;}}
    public int Score {get {return score;} set{score = value;}}
    [SerializeField] private Text textScores;
    [SerializeField] private bool playerInPowerUpState = false;//PLAYER PEGOU ALGUMA DAS PACDOT MAIORES DE PODER, (PERMITEM DIMINUIR VELOCIDADE DE INEMIGOS E PODER COMER ELE NESSE ESTADO)
    public bool PlayerInPowerUpState { get{return playerInPowerUpState;} set {playerInPowerUpState = value;}}
    [SerializeField] private bool playerIsDead = false;
    public bool PlayerIsDead{get {return playerIsDead;}set{playerIsDead = value;}}
    [SerializeField] private int lives = 3;
    public int Lives { get{return lives;}set{lives = value;}}
    [SerializeField] private SpriteRenderer[] livesTextures;
    public SpriteRenderer[] LivesTextures {get {return livesTextures;} set{livesTextures = value;}}
    //sons 
    [SerializeField] private AudioSource audioSourceForSoundEffects;
    [SerializeField] private AudioSource audioSourceForMusic;
    [SerializeField] private AudioClip audioOfMusic;
    [SerializeField] private AudioClip audioForPacdot;
    [SerializeField] private AudioClip audioForPowerPellets;
    [SerializeField] private AudioClip audioForFruits;
    [SerializeField] private AudioClip audioForPoweUpState;//estado quando o player pegou um PowerPellets
    [SerializeField] private AudioClip audioForCaughtEnemy;

    // Use this for initialization
    void Start () {
        myRigidbody2 = gameObject.GetComponent<Rigidbody2D>();
	}

    // Update is called once per frame
    void Update () {
        textScores.text = Score.ToString();
    }

    //RESET A ESTADO NORMAL (DESATIVAR APÓS UM TEMPO O EFEITO DE power up do player após pegar PowerPellets) 
    private void BackToOriginalState()
    {
        PlayerInPowerUpState = false;
        //reset a estado original audioSourceForMusic, que tinha sido mudada para fazer o efeito de power up
        audioSourceForMusic.clip = audioOfMusic;
        audioSourceForMusic.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "pacdot")
        {
            Score += 10;
            NumOfPacdotsEaten++;
            collision.gameObject.SetActive(false);

            //ativar sons
            audioSourceForSoundEffects.clip = audioForPacdot;
            audioSourceForSoundEffects.Play();
        }
        else if (collision.tag == "PowerPellet")
        {
            Score += 20;
            NumOfPacdotsEaten++;

            //ativar POWER UP do player, colocar velocidade lenta inemigos e poder comer eles
            PlayerInPowerUpState = true;
            collision.gameObject.SetActive(false);
            CancelInvoke("BackToOriginalState");//permite fazer reset ao metodo em invoke no caso de pegar varias PowerPellet sucessivamente (isto permitirá que o tempo de power up fique definido apartir de ter pegado a ultima das PowerPellets, e nao da primeria)
            Invoke("BackToOriginalState", 6f);//metodo irá permitir o player voltar a estado normal após um tempo

            //ativar sons efeito pegar PowerPellet
            audioSourceForSoundEffects.clip = audioForPowerPellets;
            audioSourceForSoundEffects.Play();
            //som temporario do estado
            audioSourceForMusic.clip = audioForPoweUpState;
            audioSourceForMusic.Play();

        }
        else if (collision.tag == "enemy")
        {
            if (!PlayerInPowerUpState)//se player nao está em estado power up 
            {
                //JOGADOR PERDE UMA VIDA
                PlayerIsDead = true;
                lives--;
            }
            else //if(PlayerInPowerUpState)//se player está em estado power up (pegou PowerPellet)
            {
                //COMER INEMIGO
 
                collision.GetComponent<Enemy>().IsActive = false;//colocar enemigo estado desativado (ou seja morto), nesse caso o GameController ira ativar ele em casa após um tempo
                collision.GetComponent<Enemy>().enabled = false;

                //ativar sons
                audioSourceForSoundEffects.clip = audioForCaughtEnemy;
                audioSourceForSoundEffects.Play();
            }
        }

        if(collision.tag == "cherry" || collision.tag == "strawberry")
        {
            Debug.Log("pegou fruta");

            if (collision.tag == "cherry")
            {
                Score += 100;
            }
            else
            {
                Score += 500;
            }

            collision.gameObject.SetActive(false);
            //ativar sons
            audioSourceForSoundEffects.clip = audioForFruits;
            audioSourceForSoundEffects.Play();
        }

    }

    private void FixedUpdate()
    {
        //MOVER 

        if (Input.GetKey(KeyCode.UpArrow))
        {
            NextDir = Vector2.up;
            Arrow = 0;
            firstTime1 = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            NextDir = Vector2.down;
            Arrow = 1;
            firstTime1 = true;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            NextDir = Vector2.left;
            Arrow = 2;
            firstTime1 = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            NextDir = Vector2.right;
            Arrow = 3;
            firstTime1 = true;
        }

        myRigidbody2.velocity = NextDir*speed;

        if (firstTime1)//a primeria vez que for pressionado um botao de direcao, se faz o player virar para o lado certo
        {
            switch (Arrow)
            {
                case 0: transform.eulerAngles = new Vector3(0, 0, 90); break;
                case 1: transform.eulerAngles = new Vector3(0, 0, 270); break;
                case 2: transform.eulerAngles = new Vector3(0, 180); break;
                case 3: transform.eulerAngles = new Vector3(0, 0); break;
            }
            firstTime1 = false;
        }


    }



}
