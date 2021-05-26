using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour {

	float speed = 20.0F; //velocidade
    float rotationSpeed = 120.0F; //velocidade de roltação
    public GameObject bulletPrefab; //GameObject que será a bala
    public Transform bulletSpawn; //pega a posição da bala

    void Update() {
        float translation = Input.GetAxis("Vertical") * speed; //controle da movimentação
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed; //controle da rotação
        
        //ajuste da velocidade em relação ao tempo dentro do jogo
        translation *= Time.deltaTime; 
        rotation *= Time.deltaTime;

        //define os eixos de alteração em relação à movimentação do player
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);

        if(Input.GetKeyDown("space")) //atirar com barra de espaço
        {
            //definição do local e posição da colocação da bala em cena
            GameObject bullet = GameObject.Instantiate(bulletPrefab, 
                bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            
            //física da bala
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);
        }
    }    
}
