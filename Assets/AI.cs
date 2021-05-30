using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    public Transform player; //pega infos do jogador
    public Transform bulletSpawn; //posição do spawn da bala
    public Slider healthBar;   //define formato da barra de vida
    public GameObject bulletPrefab; //define o gameObject que representará a bala

    NavMeshAgent agent;
    public Vector3 destination; // The movement destination.
    public Vector3 target;      // The position to aim to.
    float health = 100.0f;      //vitalidade total
    float rotSpeed = 5.0f;      //velocidade de rotação

    float visibleRange = 80.0f;
    float shotRange = 40.0f;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>(); //acessa o componente NavMesh
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        InvokeRepeating("UpdateHealth", 5, 0.5f); //atualização da barra de vida
    }

    void Update()
    {
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position); //renderiza a barra de vida
        healthBar.value = (int)health; //valor corrente da barra de vida
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0); //posição da barra de vida
    }

    void UpdateHealth() //atualiza a barra de vida
    {
        if (health < 100)
            health++;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "bullet") //tira 10 de vida quando colide com a bala
        {
            health -= 10;
        }
    }

    [Task]
    public void PickRandomDestination() //selecionar destino aleatório no mapa
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)); //limites do random
        agent.SetDestination(dest);//define a variável de destino
        Task.current.Succeed();//conclui e segue a próxima tarefa
    }
    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected) //exibe no componente do local do agente
        {
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        }

        //se chegou no local com certa distânca do programado completa a ação atual
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }
    [Task]
    public void PickDestination(int x, int z)
    {   //escolhe o destino dentre os que estão no Patrol.BT
        Vector3 dest = new Vector3(x, 0, z); //feine o vetor até o destino
        agent.SetDestination(dest);//seta na componente 
        Task.current.Succeed();
    }
    [Task]
    public void TargetPlayer()//define o player como alvo
    {
        target = player.transform.position;//pega posição do player
        Task.current.Succeed();
    }
    [Task]
    public bool Fire()//atirar
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab,//instancia a bala 
            bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2500);//define a física da bala
        return true;
    }
    [Task]
    public void LookAtTarget()//olha um alvo
    {
        Vector3 direction = target - this.transform.position; //define o vetor entre o NPC e o alvo
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,//rotaciona o NPC na direção do alvo
            Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        if (Task.isInspected) Task.current.debugInfo = string.Format("angle={0}", //verificação do funcionamento
            Vector3.Angle(this.transform.forward, direction));

        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)//define o ângulo da rotação
        { Task.current.Succeed(); }
    }
    [Task]
    bool SeePlayer() //olha para a posição do player
    {
        Vector3 distance = player.transform.position - this.transform.position; //define o vetor NPC e Player
        RaycastHit hit; //colocação do raycast para localizar o player
        bool seeWall = false;//define variável boolena para impedir perseguição quando player atrás da parede
        Debug.DrawRay(this.transform.position, distance, Color.red);//colorir o raycast

        if (Physics.Raycast(this.transform.position, distance, out hit))
        {   //se o raycast detectar parede ele não segue
            if (hit.collider.gameObject.tag == "wall")
            { seeWall = true; } 
        }
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("wall={0}", seeWall); //verificação do método

        //ativa o método se a distância do player menor que da parede
        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }
    [Task]
    bool Turn(float angle) 
    {   //define a varável usada para o rotação do NPC
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward; 
        target = p;
        return true;
    }
    [Task] 
    public bool IsHealthLessThan(float health) //verificação da vida
    { 
        return this.health < health; 
    }
    [Task] 
    public bool Explode() //robô desaparece da cena quando morre
    { 
        Destroy(healthBar.gameObject); //exclui a barra de vida da cena
        Destroy(this.gameObject); //exclui prefab do robô quando morre
        return true; 
    }
}
