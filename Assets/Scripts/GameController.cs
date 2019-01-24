using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {


    [SerializeField] private Text textScores; 
    private GameObject player;
    private Vector3 originalPositionOfPlayer;
    private GameObject[] enemys;
    private Player playerComponent;
    [SerializeField] private Text TextResetGame;
    [SerializeField] private Text TextFeedBack;
    [SerializeField] private Text TextYouLostALive;
    private GameObject[] allPacDots;
    private GameObject[] allPowerPellets;
    private List<GameObject> allFruits=new List<GameObject>();
    private bool firstTime1 = true;
    private bool firstTime2 = true;
    [SerializeField] private List<GameObject> originPointsForGameCharacters=new List<GameObject>();//ARMAZENA OS PONTOS DE ORIGEM ONDE PODEM APARECER OS INEMIGOS DE JEITO RANDOM, TAMANHO DEVE SER IGUAL QUE NUMERO DE INEMIGOS
    [SerializeField] private List<GameObject> originPointsForFruits = new List<GameObject>();//ARMAZENA OS PONTOS DE ORIGEM ONDE PODEM APARECER AS FRUTAS DE JEITO RANDOM
    private List<GameObject> originPointsForFruitsRandom = new List<GameObject>();
    private int originalLivesOfPLayer;
    [SerializeField] private GameObject home;//lugar no centro da cena, onde voltam a aparecer os inemigos após serem comidos pelo player 
    [SerializeField] private Sprite spriteForEnemyAtHome;//sprite usado para mudar a cor de enemigos em casa
    private Vector2 nextPositionOfEnemy;
    private bool authorizeMoveEnemiesAtHome=true;
    //sons
    [SerializeField] private AudioSource audioSourceForMusic;
    [SerializeField] private AudioClip audioOfMusic;
    [SerializeField] private AudioClip audioYouWin;
    [SerializeField] private AudioClip audioYouLostALive;
    [SerializeField] private AudioClip audioGameOver;

    private void Awake()
    {
        player = GameObject.Find("pacman");
        playerComponent = player.GetComponent<Player>();
        originalPositionOfPlayer = player.transform.position;
        
        enemys = GameObject.FindGameObjectsWithTag("enemy");

        allPacDots = GameObject.FindGameObjectsWithTag("pacdot");
        allPowerPellets = GameObject.FindGameObjectsWithTag("PowerPellet");

        GameObject[] allFruit1 = GameObject.FindGameObjectsWithTag("cherry");
        GameObject[] allFruit2 = GameObject.FindGameObjectsWithTag("strawberry");
        allFruits.AddRange(allFruit1);
        allFruits.AddRange(allFruit2);
    }

    // Use this for initialization
    void Start () {
        originalLivesOfPLayer = playerComponent.Lives;

        Initialize(0);

        TextYouLostALive.color = Color.red;
        TextResetGame.color = Color.white;
        
        nextPositionOfEnemy = (Vector2)home.transform.position+ new Vector2(0.8f,0);
    }

    private IEnumerator ActivateAndDesactivateFruits()
    {
        //activar frutas
        for (int i = 0; i < allFruits.Count; i++)
        {  
            yield return new WaitForSeconds(10f);
            allFruits[i].SetActive(true);
            allFruits[i].transform.position = originPointsForFruitsRandom[i].transform.position;
        }
    }

    private IEnumerator ContinueGame()
    {
        yield return new WaitForSeconds(2f);
        Initialize(1);//inicializar o jogo após perder vida
        firstTime1 = true;
    }

    private IEnumerator BackAtHome(GameObject enemy)//metodo que permite reativar os inimigos em casa (no centro da cana), após um tempo.
    {
        enemy.GetComponent<Enemy>().enabled = false;
        enemy.transform.position = home.transform.position;
        enemy.GetComponent<SpriteRenderer>().sprite = spriteForEnemyAtHome;
        enemy.GetComponent<Enemy>().IsActive = true;//NOTA: IMPORTANTE COLOCAR ESTA LINHA ANTES DA PAUSA DA COROUTINE

        yield return new WaitForSeconds(6f);//exucutar após 5 segundos

        enemy.GetComponent<Enemy>().enabled = true;
        enemy.GetComponent<Enemy>().FirstTime1 = true;
        enemy.GetComponent<Enemy>().FirstTime2 = true;
    }


    private void FixedUpdate()
    {
        for (int i = 0; i < enemys.Length; i++)
        {
            //RESET INEMIGOS EM CASA(HOME, centro da cena) A PRIMEIRA VEZ QUE SEU VALOR IsActive FOR TRUE
            if (!enemys[i].GetComponent<Enemy>().IsActive)//quando nao estiver ativo (foi comido), entao chamar metodo para reativar ele em casa (centro da cena)
            {
                //NOTA:O QUE FOR COLOCADO NESTE TRECHO SERA EXECUTADO UMA UNICA VEZ, devido a que IsActive es mudada de valor antes da pausa da coroutine
                StartCoroutine(BackAtHome(enemys[i]));
            }
            if (enemys[i].GetComponent<SpriteRenderer>().sprite == spriteForEnemyAtHome && authorizeMoveEnemiesAtHome)//a condicao de enemigos terem o sprite de casa, é usada para definir quando executar o codigo de mover eles automaticamente de esquerda para direita
            {
                //MOVER ENEMIGOS AUTOMATICAMENTE DE ESQUERDA PARA DIREITA ENQUANTO ESTIVEREM EM CASA 
                Vector2 positonOfEnemy = enemys[i].transform.position;
                if (positonOfEnemy != nextPositionOfEnemy)
                {
                    Vector2 p = Vector2.MoveTowards(positonOfEnemy, nextPositionOfEnemy, 0.022f);
                    enemys[i].GetComponent<Rigidbody2D>().MovePosition(p);
                }
                else//quando chegou na ponto final, entao atualizar ele mudando para o sentido contrario
                {
                    nextPositionOfEnemy = new Vector2(-nextPositionOfEnemy.x, nextPositionOfEnemy.y);
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {


        if (playerComponent.PlayerIsDead)
        playerComponent.LivesTextures[playerComponent.Lives].enabled = false;//DESATIVAR TEXTURAS DE VIDA DO PLAYER, CADA VEZ QUE PERDER UMA

        if ((playerComponent.PlayerIsDead) &&  (playerComponent.Lives > 0) && firstTime1)//SE JOGADOR PERDER UMA VIDA //(playerComponent.Lives > 1)
        {
            authorizeMoveEnemiesAtHome = false;

            //ativar sons 
            audioSourceForMusic.clip = audioYouLostALive;
            audioSourceForMusic.loop = false;
            audioSourceForMusic.Play();

            //PAUSAR CODIGO DE ELEMENTOS DO JOGO (neste caso nao se pausa o jogo com Time.timeScale = 0; como acontece quando o jogador perde o jogo) //NOTA: LEMBRAR NAO PAUSAR TEMPO USANDO COROUTINES
            TextYouLostALive.gameObject.SetActive(true);

            //DESATIVAR CODIGO DOS ENIMIGOS. 
            foreach(GameObject ememy in enemys)
            {
                ememy.GetComponent<Enemy>().enabled = false;
            }
            //DESATIVAR CODIGO DO PLAYER
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;//reset velocidade 0, para nao ficar na ultima que tinha
            playerComponent.enabled = false;

            StopAllCoroutines();//NOTA: NECESSARIO FAZER RESET NAS COROUTINES, DEVIDO A QUE PODE SE EXECUTAR A COROUTINE DO BackAtHome APÓS PERDER UMA VIDA (isso ativaria enemigos em casa, enquanto os outros estam desativados). LEMBRAR QUE QUANDO SE PEDER VIDAS, NAO SE PAUSA O JOGO COMO NO CASO DE FINALIZAR A PARTIDA, E AO INVES DISSO SÓ SE DESATIVA O CODIGO DOS ENEMIGOS MOMENTANEAMENTE

            StartCoroutine("ContinueGame");//metodo que voltará a ativar as coisas automaticamente, após certo tempo

           firstTime1 = false;
        }

        if((playerComponent.NumOfPacdotsEaten >= playerComponent.TotalOfPacdots || playerComponent.Lives<=0) && firstTime2)//(PARTIDA FINALIZADA) SE JOGADOR GANHOU OU PERDEU JOGO 
        {
            authorizeMoveEnemiesAtHome = false;

            //ativar textos FeedBack 
            if (playerComponent.NumOfPacdotsEaten >= playerComponent.TotalOfPacdots)
            {
                TextFeedBack.text = "YOU WIN";
                TextFeedBack.color = Color.blue;
                textScores.text = playerComponent.Score.ToString();//nota: necessario atualizar o valor devido a que o jogo será pausado e ficaria sem atualizar o texto a o novo valor
                //ativar sons ganhar
                audioSourceForMusic.clip = audioYouWin;
                audioSourceForMusic.loop = false;
                audioSourceForMusic.Play();
            }
            else if (playerComponent.Lives <= 0)
            {
                TextFeedBack.text = "GAME OVER";
                TextFeedBack.color = Color.red;
                //ativar sons perder jogo
                audioSourceForMusic.clip = audioGameOver;
                audioSourceForMusic.loop = false;
                audioSourceForMusic.Play();
            }
            
            Time.timeScale = 0;//pausar jogo

            //DESATIVAR INEMIGOS
            //--
            for (int i = 0; i < enemys.Length; i++)
            {
                enemys[i].gameObject.SetActive(false);
            }       
            player.SetActive(false);//DESATIVAR PLAYER
            //--

            TextFeedBack.gameObject.SetActive(true);
            TextResetGame.gameObject.SetActive(true);//ativa texto de reset, que vai permitir despausar jogo quando for pressionado

            firstTime2 = false;
        }


        if (TextResetGame.GetComponent<OnClick>().Click)//se for pressionado o botao de reset jogo
        {
            Initialize(0);//inicializar após perder o jogo ou ganhar
            TextResetGame.GetComponent<OnClick>().Click = false; 
            firstTime2 =true;
        }


    }

    private void Initialize(int TypeOfinitialization)//0: inicializar jogo após perder jogo ou ganhar jogo, 1: inicializar jogo após perder vida  
    {
        //CODIGO NECESSARIO PARA POSICIONAR AS 4 PERSONAGENS EM 4 PONTOS RANDOM (inemigos)
        //TRECHO PARA OBTER A LISTA originPointsForGameChacartersRandom usadas nas operacoes posteriores
        //permite desordenar lista originPointsForGameCharacters, usada para permitir os 4 enemigos do jogo aparecerem em 4 pontos ramdom 
        //----   
        List<GameObject> ToriginPointsForGameChacarters = new List<GameObject>();//usamos lista temporaria com os mesmos elementos da lista originPointsForGameChacarters (esta lista será necessaria devido a que serao removidos os elementos da lista no processo, e nao queremos remover a lista original)
        for (int i=0; i < originPointsForGameCharacters.Count; i++)
        {
            ToriginPointsForGameChacarters.Add(originPointsForGameCharacters[i]);
        }
        List<GameObject> originPointsForGameChacartersRandom = new List<GameObject>();
        while(ToriginPointsForGameChacarters.Count >0)
        {
            int randomVal = Random.Range(0, ToriginPointsForGameChacarters.Count);

            originPointsForGameChacartersRandom.Add(ToriginPointsForGameChacarters[randomVal]);
            ToriginPointsForGameChacarters.RemoveAt(randomVal);
        }
        //----    

        //INICIALIZAR INEMIGOS
        //----
        for (int y = 0; y < enemys.Length; y++)
        {
            enemys[y].transform.position = originPointsForGameChacartersRandom[y].transform.position;

            enemys[y].gameObject.SetActive(true);//ATIVAR INEMIGOS

            //FAZER RESET A VARIAVEIS NECESSARIAS DE SCRIPT ENEMY, PARA PERMITIR OS INEMIGOS VOLTAR A INICIAR SEU PERCURSO DESDE NOVOS PONTOS DE ORIGEM AO INVES DE CONSIDERANDO A ULTIMA POSICAO ATUAL (ULTIMO wayPoint usado)       
            //----
            enemys[y].GetComponent<Enemy>().FirstTime1 = true;//FirstTime1 a true, permite fazer reset a actualWayPointContainer. actualWayPointContainer É NECESSARIO para "OBTER WayPoint" inicial, em FixedUpdate(). a partir desse WayPoint é que sao achados os proximo WayPoints (nextWayPoints)
            enemys[y].GetComponent<Enemy>().FirstTime2 = true;//FirstTime2 a true, permite fazer reset o codigo "OBTER proximo wayPoint" em FixedUpdate(), que permite escolher o proximo wayPoint dos disponiveis
            enemys[y].GetComponent<Enemy>().IsActive = true;
            //----

            enemys[y].GetComponent<Enemy>().enabled = true;//despausar codigo de cada inemigo, se estiver pausado
        }
        //----

        //INICIALIZAR PLAYER
        //----
        player.SetActive(true);//ATIVAR PLAYER 
        playerComponent.enabled = true;//despausar codigo de player

        player.transform.position = originalPositionOfPlayer;
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;//reset velocidade 0, para nao ficar na ultima que tinha antes de pausar
        player.transform.eulerAngles = Vector3.zero;//reset angulo ao original
        playerComponent.PlayerIsDead = false;
        playerComponent.NextDir = Vector2.zero;
        playerComponent.PlayerInPowerUpState = false;
        //----

        if (TypeOfinitialization == 0)//executa só se inicializar jogo após finalizar partida (após perder jogo ou ganhar jogo, nao quando perder vida)
        {
            playerComponent.Score = 0;
            playerComponent.NumOfPacdotsEaten=0;
            playerComponent.Lives = originalLivesOfPLayer;
            foreach (SpriteRenderer liveTexture in playerComponent.LivesTextures)
            {
                liveTexture.enabled= true;
            }

            foreach (GameObject pacdot in allPacDots)
            {
                pacdot.gameObject.SetActive(true);
            }
            foreach(GameObject powerPellet in allPowerPellets)
            {
                powerPellet.gameObject.SetActive(true);
            }    
            Time.timeScale = 1;//despausar tempo do jogo
        }

        foreach (GameObject fruit in allFruits)
        {
            fruit.gameObject.SetActive(false);
        }
        //CODIGO PARA POSICIONAR FRUTAS EM ORDEM RANDOM EM DOIS PONTOS POSIVEIS ATRIBUIDOS NA LISTA originPointsForFruits (partindo do processo de desordenar a lista originPointsForFruits usando uma lista temporaria com os mesmos elementos)
        List<GameObject> ToriginPointsForFruits = new List<GameObject>();//usamos lista temporaria com os mesmos elementos da lista originPointsForFruits (esta lista será necessaria devido a que serao removidos os elementos da lista no processo, e nao queremos remover a lista original)
        for (int i = 0; i < originPointsForFruits.Count; i++)
        {
            ToriginPointsForFruits.Add(originPointsForFruits[i]);
        }
        originPointsForFruitsRandom.Clear();
        while (ToriginPointsForFruits.Count > 0)
        {
            int randomVal2 = Random.Range(0, ToriginPointsForFruits.Count);

            originPointsForFruitsRandom.Add(ToriginPointsForFruits[randomVal2]);
            ToriginPointsForFruits.RemoveAt(randomVal2);
        }
        StartCoroutine("ActivateAndDesactivateFruits");

        TextResetGame.gameObject.SetActive(false);
        TextFeedBack.gameObject.SetActive(false);
        TextYouLostALive.gameObject.SetActive(false);

        authorizeMoveEnemiesAtHome = true;

        //reset audioSourceForMusic a seu estado original, uma vez que é trocado quando finalizar partida (o jogador ganhar ou perder o jogo), ou ainda se perder uma vida
        audioSourceForMusic.clip = audioOfMusic;
        audioSourceForMusic.loop = true;
        audioSourceForMusic.Play();

    }


}
