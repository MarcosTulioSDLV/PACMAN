using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {


    private GameObject player;//pacman
    private Player playerCompontent;
    private Rigidbody2D myRigidbody2D;
    private SpriteRenderer mySpriteRenderer;
    [SerializeField] private float speed = 0.025f;//valor default é: speed Blinky (normal: 0.025f, rapido:0.026f)
    public float speedWithPowerUpState=0.015f;//velocidade quando o player esta no estado power up (comeu uma PowerPellet)
    private float originalSpeed;
    private float maxSpeed=0.026f;//0.03f//usada para Blinky e Pinky (VELOCIDADE USADA QUANDO O PLAYER COMEU MAIS DA MITADE DAS pacdots)
    private float normalSpeedOfBlinky=0.025f;//usada para Pinky (NOTA IMPORTANTE:"DEVE SER COLOCADA SEMPRE IGUAL DO QUE A DE BLINKY NO INSPETOR") 
    [SerializeField] private enum TypeOfEnemy { Blinky, Clyde, Inky, Pinky };
    [SerializeField] private TypeOfEnemy typeOfEnemy;
    [SerializeField] private Sprite spriteOfBlinky;//sprite necessario para mudar a imagem de Pinky para a de Blinky 
    [SerializeField] private Sprite newSpriteForAllEnemys; //sprite para mudar os enemigos a azul, quando player está em power up (pegou PowerPellets) 
    private Sprite originalSprite;
    [SerializeField] private List<WayPointsContainer> listWayPointsContainers;// usado como Container de estruturas que possuem: O wayPoint, e uma lista dos "PROXIMOS wayPoints" destino.
                                                                              //UM "PROXIMO wayPoint", É DEFINIDO COMO UM wayPoint AO QUAL O INEMIGO PODE IR COMO PROXIMO DESTINO. 
    private WayPointsContainer actualWayPointContainer;//usado para armazenar o atual wayPointContainer, que esta sendo analizado dos disponiveis na lista listWayPointsContainers

    private GameObject anterior;
    private bool firstTime1 = true;
    public bool FirstTime1 {get{return firstTime1;}set{firstTime1 = value;}}
    private bool firstTime2 = true;
    public bool FirstTime2 {get{return firstTime2;} set{firstTime2 = value;}}
    private GameObject next;
    private bool isActive=true;//esta vivo o inemigo, nao tem sido comido 
    public bool IsActive{get{return isActive;} set {isActive = value;}}


    private void Awake()
    {
        player = GameObject.Find("pacman");
        playerCompontent = player.GetComponent<Player>();

        myRigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        mySpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start() {
        originalSprite = mySpriteRenderer.sprite;//usado para mudar imagem de Pinki
        originalSpeed = speed;
    }

    // Update is called once per frame
    void Update() {

    }

    private void FixedUpdate()
    {
        //CODIGO PARA OBTER WayPoint ORIGINAL (APARTIR DESTE É QUE SAO OBTIDOS OS PROXIMOS wayPoint AONDE PODERIA IR O INEMIGO, E DOS QUAIS É ESCOLHIDO UM COMO O PROXIMO DESTINO, A ESCOLHA É POR CRITERIO DEPENDENDO DO TIPO DE INEMIGO)
        if (FirstTime1)
        {
            //codigo usado para obter o wayPoint origen do percurso, (wayPoint MAIS PERTO DO INEMIGO), (é o wayPoint do qual o codigo assume que vai comecar o percurso o inemigo).
            // o enimigo deve ser sempre posicionado num wayPoit para comecar seu percurso devido a que o codigo faz o inemigo ir de um wayPoint para um proximo wayPoint continuamente)
            //----
            List<GameObject> allWayPoints = new List<GameObject>();
            for (int i = 0; i < listWayPointsContainers.Count; i++)
            {
                if (!allWayPoints.Contains(listWayPointsContainers[i].WayPoint))
                {
                    allWayPoints.Add(listWayPointsContainers[i].WayPoint);
                }
            }

            GameObject wayPointOrigin = GetWayPointCloserToElement(allWayPoints, gameObject);//obter o wayPoint MAIS PERTO DO INEMIGO (wayPoint origem do percurso para o inemigo) 

            //com wayPointOrigin já encontrado, é procurado o WayPointContainer original, que será usado a primeira vez  em FixedUpdate() no trecho para "OBTER proximo WayPoint"
            //actualWayPointContainer É NECESSARIO para "OBTER WayPoint" inicial, em FixedUpdate(). a partir desse WayPoint é que sao achados os proximo WayPoints
            WayPointsContainer originalWayPointContainer = listWayPointsContainers[0];
            for (int y = 0; y < listWayPointsContainers.Count; y++)
            {
                if (GameObject.ReferenceEquals(listWayPointsContainers[y].WayPoint, wayPointOrigin))
                    originalWayPointContainer = listWayPointsContainers[y];
            }

            actualWayPointContainer = originalWayPointContainer;//inemigo comeca percurso desde o wayPoint mais perto a ele que foi encontrado //usando ao inves /*listWayPointsContainers[0]*/ enemy comeca percurso desde primeiro WayPoint do primiero elemento atribuido na listWayPointsContainers  
            anterior = actualWayPointContainer.WayPoint;
            //----

            FirstTime1 = false;
        }

        //"OBTER proximo wayPoint", USADO COMO PROXIMO DESTINO DO INEMIGO E ESCOLHIDO DE UMA LISTA DOS proximos wayPoint, (É ESCOLHIDO SEGUINDO UM CRITERIO ESPECIFICO POR CADA INEMIGO)
        //O proximo WayPoint SERA ARMAZENA EM NEXT
        //-----
        if (FirstTime2)
        {
            List<GameObject> nextsWayPoint = actualWayPointContainer.NextsWayPoints;//obtemos lista de proximos wayPoint (UM PROXIMO WayPoint É DEFINIDO COMO UM wayPoint AO QUAL O INEMIGO PODE IR COMO PROXIMO DESTINO). 
                                                                                    //assim na lista nextsWayPoint sao obtidos todos os wayPoint aos que o inemigo poderá ir como proximo destino.

            List<GameObject> nextsWayPointValid = new List<GameObject>();// lista usada para armazenar wayPoints validos como proximo destino (todos menos o atual, para evitar regresar ao mesmo ponto)

            //acrescenta na lista nextsWayPointValid, os wayPoints validos como proximo destino (todos menos o atual, para evitar regresar ao mesmo ponto)
            for (int i = 0; i < nextsWayPoint.Count; i++)
            {
                if (!GameObject.ReferenceEquals(nextsWayPoint[i], anterior))
                {
                    nextsWayPointValid.Add(nextsWayPoint[i]);
                }
            }

            if (typeOfEnemy == TypeOfEnemy.Blinky)//se o inemigo escolhido for Blinky (é veloz e persegue o pacman)
            {
                if (playerCompontent.NumOfPacdotsEaten >= (playerCompontent.TotalOfPacdots/2))//se pacdots comidos pelo player forem  >= mitade da quantidade total (referentes a ter comido a mitade dos pacdot) 
                {
                    followPlayerQuickly(nextsWayPointValid);//PERSEGUINDO RAPIDO
                }
                else
                {
                    followPlayerWithNormalSpeed(nextsWayPointValid);//PERSEGUINDO NORMAL
                }        
            }
            else if (typeOfEnemy == TypeOfEnemy.Clyde)//se o inemigo escolhido for Clyde (se movimenta de forma ramdom pelo cenario)
            {
                next = nextsWayPointValid[Random.Range(0, nextsWayPointValid.Count)];//obtemos o next wayPoint (o proximo  WayPoint aonde o inimego irá como proximo destino), usando criterio de caminho ramdom, necessario para Clyde 
            }
            else if (typeOfEnemy == TypeOfEnemy.Inky)//se o inemigo escohido for Inky (seu movimento é imprevisível pelo cenario, pode perseguir o player, evita-lo ou ir em direcao random)
            {          
                int typeOfMovimeto =0;// 0: perseguir palyer, 1: evitar player, 2: random 

                int randomVal = Random.Range(1, 11);
                switch (randomVal)
                {
                    case 1:
                    case 2:
                    case 3: typeOfMovimeto = 0; break;
                    case 4: 
                    case 5: 
                    case 6: 
                    case 7: typeOfMovimeto = 1; break;
                    case 8: 
                    case 9:
                    case 10: typeOfMovimeto = 2; break;
                }

                if(typeOfMovimeto == 0)//30% de probabilidade para PERSEGUIR PLAYER
                {
                    //PERSIGUE
                    next = GetWayPointCloserToElement(nextsWayPointValid, player);//obtemos o next wayPoint (o proximo wayPoint aonde o inemigo irá como proximo destino), usando criterio de perseguir o player, necessario para Inky
                }
                else if (typeOfMovimeto==1)//40% de probabilidade para EVITAR PLAYER
                {
                   //EVITA
                    if (nextsWayPointValid.Count > 1)//tem varios nextWayPoint validos na lista
                    {
                        GameObject wayPointCloserToElement = GetWayPointCloserToElement(nextsWayPointValid, player);

                        List<GameObject> nextsWayPointWithoutWayPointCloserToElement = new List<GameObject>();//lista que ira conter todos os proximos wayPoint validos (da lista nextsWayPointValid), sem incluir o mais perto do player 
                        for (int i = 0; i < nextsWayPointValid.Count; i++)
                        {
                            if (!ReferenceEquals(nextsWayPointValid[i], wayPointCloserToElement))
                            {
                                nextsWayPointWithoutWayPointCloserToElement.Add(nextsWayPointValid[i]);
                            }
                        }

                        if (nextsWayPointWithoutWayPointCloserToElement.Count > 1)//tem varios nextWayPoint validos na lista
                        {
                            next = nextsWayPointWithoutWayPointCloserToElement[Random.Range(0, nextsWayPointWithoutWayPointCloserToElement.Count)];
                        }
                        else if(nextsWayPointWithoutWayPointCloserToElement.Count==1)//tem um unico nextWayPoint valido na lista
                        {
                            next = nextsWayPointWithoutWayPointCloserToElement[0];
                        }

                    }
                    else if(nextsWayPointValid.Count==1)//tem um unico nextWayPoint valido na lista
                    {
                        next = nextsWayPointValid[0];
                    }
                }
                else if(typeOfMovimeto==2)//30% de probabilidade para SEGUIR CAMINHO RANDOM
                {
                    //RANDOM
                    next = nextsWayPointValid[Random.Range(0, nextsWayPointValid.Count)];//obtemos o next wayPoint (o proximo  WayPoint aonde o inemigo irá como proximo destino), usando criterio de caminho ramdom, necessario para Inky
                }
            }
            else if (typeOfEnemy == TypeOfEnemy.Pinky)//se o inemigo escolhido for Pinky (se movimenta de forma ramdom enquanto estiver longo do Player, e procura ele enquanto estiver perto)
            {
                float distanceForEnemyFollowPlayer = 3.75f;
                float distance = Vector2.Distance(transform.position,player.transform.position);//obtem distancia emtre inemigo e player

                if (distance <= distanceForEnemyFollowPlayer)
                {
                    //PERSEGUIR IGUAL QUE Blinky
                    //obtemos o next wayPoint (o proximo wayPoint aonde o inemigo irá como proximo destino), usando criterio de perseguir o player, necessario para Pinky
                    if (playerCompontent.NumOfPacdotsEaten >= (playerCompontent.TotalOfPacdots/2))//se pacdots comidos pelo player forem  >= mitade da quantidade total (referentes a ter comido a mitade dos pacdot)  
                    { 
                        followPlayerQuickly(nextsWayPointValid);
                    }
                    else
                    {  
                        followPlayerWithNormalSpeed(nextsWayPointValid);
                    }
                }
                else
                {
                    //SEGUIR CAMINHO RANDOM
                    next = nextsWayPointValid[Random.Range(0, nextsWayPointValid.Count)];//obtemos o next wayPoint (o proximo  WayPoint aonde o inemigo irá como proximo destino), usando criterio de caminho ramdom, necessario para Pinky
                }

            }

            FirstTime2 = false;
        }
        //-----     

        //MUDARA A VELOCIDADE E SPRITES
        //----
        if (typeOfEnemy == TypeOfEnemy.Blinky)//se o inemigo escolhido for Blinky (é veloz e persegue o pacman)
        {

            if (playerCompontent.PlayerInPowerUpState)
            {
                mySpriteRenderer.sprite = newSpriteForAllEnemys;
                speed = speedWithPowerUpState;
            }
            else if (playerCompontent.NumOfPacdotsEaten >= (playerCompontent.TotalOfPacdots/2))//se pacdots comidos pelo player forem  >= mitade da quantidade total (referentes a ter comido a mitade dos pacdot) 
            {
                mySpriteRenderer.sprite = originalSprite;
                speed = maxSpeed;
            }
            else//modo normal
            {
                mySpriteRenderer.sprite = originalSprite;
                speed = originalSpeed;
            }

        }
        else if (typeOfEnemy == TypeOfEnemy.Clyde)
        {
            if (playerCompontent.PlayerInPowerUpState)
            {
                speed = speedWithPowerUpState;
                mySpriteRenderer.sprite = newSpriteForAllEnemys;
            }
            else
            {
                speed = originalSpeed;
                mySpriteRenderer.sprite = originalSprite;
            }
        }
        else if (typeOfEnemy == TypeOfEnemy.Inky)
        {
            if (playerCompontent.PlayerInPowerUpState)
            {
                speed = speedWithPowerUpState;
                mySpriteRenderer.sprite = newSpriteForAllEnemys;
            }
            else
            {
                speed = originalSpeed;
                mySpriteRenderer.sprite = originalSprite;
            }
        }
        else if (typeOfEnemy == TypeOfEnemy.Pinky)
        {
            float distanceForEnemyFollowPlayer = 3.75f;
            float distance = Vector2.Distance(transform.position, player.transform.position);//obtem distancia emtre inemigo e player

            if (playerCompontent.PlayerInPowerUpState)
            {
                mySpriteRenderer.sprite = newSpriteForAllEnemys;
                speed = speedWithPowerUpState;
            }
            else if (playerCompontent.NumOfPacdotsEaten >= (playerCompontent.TotalOfPacdots / 2))//se pacdots comidos pelo player forem  >= mitade da quantidade total (referentes a ter comido a mitade dos pacdot) 
            {
                if (distance <= distanceForEnemyFollowPlayer)
                {
                    mySpriteRenderer.sprite = spriteOfBlinky;//sprite de Blinky
                    speed = maxSpeed;
                }
                else
                {
                    mySpriteRenderer.sprite = originalSprite;
                    speed = originalSpeed;
                }
            }
            else//modo normal
            {
                if (distance <= distanceForEnemyFollowPlayer)
                {
                    mySpriteRenderer.sprite = spriteOfBlinky;//sprite de Blinky
                    speed = normalSpeedOfBlinky;//NOTA:conferir que normalSpeedOfBlinky tenha a mesma velocidade que a do blinky
                }
                else
                {
                    mySpriteRenderer.sprite = originalSprite;
                    speed = originalSpeed;
                }
            }
        }
        //----

        //MOVER INEMIGO DESDE POSICAO ATUAL ATÉ O PROXIMO wayPoint ENCONTRADO no trecho anterior
        //-----
        Vector2 actualPosition = transform.position;
        Vector2 nextWayPointPosition = next.transform.position;

        if (actualPosition != nextWayPointPosition)//inemigo ainda nao está na position destino encontrada (proximo wayPoint)
        {
            Vector2 p = Vector2.MoveTowards(actualPosition, nextWayPointPosition, speed);
            myRigidbody2D.MovePosition(p);
        }
        else//inemigo chegou na posicao destino encontrada (proximo wayPoint)
        {
            //trecho permite fazer reset ao processo de obter o proximo WayPoint, atualizando todas as variaveis necessarias
            //----
            anterior = actualWayPointContainer.WayPoint;//atualizar anterior wayPoint

            //atualizar actualWayPointContainer 
            for (int i = 0; i < listWayPointsContainers.Count; i++)
            {
                if (GameObject.ReferenceEquals(listWayPointsContainers[i].WayPoint, next))//se a wayPoint é igual do que o proximo, obtemos o container nesse indice para atualizá-lo
                {
                    actualWayPointContainer = listWayPointsContainers[i];
                    break;
                }
            }
            FirstTime2 = true;
            //----
        }
        //-----

    }

    private GameObject GetWayPointCloserToElement(List<GameObject> wayPoints, GameObject element)//metodo usado para obter o WayPoint (dos WayPoints dum lista), que estiver mais perto dum elemento (usado para player ou inemigo)
    {
        //inicializar valores
        float minDistance = Vector2.Distance(element.transform.position, wayPoints[0].transform.position);
        GameObject wayPointCloserToElement = wayPoints[0];

        for (int i = 0; i < wayPoints.Count; i++)
        {
            GameObject actualWayPoint = wayPoints[i];

            float actualDistance = Vector2.Distance(element.transform.position, actualWayPoint.transform.position);
            if (actualDistance <= minDistance)//se distancia atual for menor ou igual do que distancia ja armazenada como a distancia menor, entao atualizar a distancia menor e o WayPoint mais perto do elemento 
            {
                minDistance = actualDistance;
                wayPointCloserToElement = actualWayPoint;
            }
        }
        return wayPointCloserToElement;
    }

    private void followPlayerWithNormalSpeed(List<GameObject> nextsWayPointValid)//metodo com 60 % de probabilidade para: inemigo perseguir o player x inemigo ir em direcao random
    {       
        //PERSEGUIR 60% DE INTENSIDADE
        bool perseguir = true;
        int randomVal = Random.Range(1, 11);
        switch (randomVal)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6: perseguir = true; break;
            case 7: 
            case 8:
            case 9:
            case 10: perseguir = false; break;
        }
        if (perseguir)//60% de probabilidade para PERSEGUIR PLAYER
        {
            next = GetWayPointCloserToElement(nextsWayPointValid, player);//obtemos o next wayPoint (o proximo wayPoint aonde o inemigo irá como proximo destino), usando criterio de perseguir o player, necessario para Blinky
        }
        else//probabilidade menor restante para SEGUIR CAMINHO RANDOM
        {
            next = nextsWayPointValid[Random.Range(0, nextsWayPointValid.Count)];//as vezes (com menor porcentagem) usa-se o criterio ramdom para nao ficar sempre em todo momento perseguindo player
        }
    }

    private void followPlayerQuickly(List<GameObject> nextsWayPointValid)
    {
        //PERSEGUIR 70% DE INTENSIDADE
        bool perseguir = true;
        int randomVal = Random.Range(1, 11);
        switch (randomVal)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7: perseguir = true; break;
            case 8:
            case 9: 
            case 10: perseguir = false; break;
        }
        if (perseguir)//70% de probabilidade para PERSEGUIR PLAYER
        {
            next = GetWayPointCloserToElement(nextsWayPointValid, player);//obtemos o next wayPoint (o proximo wayPoint aonde o inemigo irá como proximo destino), usando criterio de perseguir o player, necessario para Blinky
        }
        else//probabilidade menor restante para SEGUIR CAMINHO RANDOM
        {
            next = nextsWayPointValid[Random.Range(0, nextsWayPointValid.Count)];//as vezes (com menor porcentagem) usa-se o criterio ramdom para nao ficar sempre em todo momento perseguindo player
        }

    }

}
