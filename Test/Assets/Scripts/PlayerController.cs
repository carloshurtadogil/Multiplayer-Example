using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public GameObject bulletObject;
    public Transform bulletSpawn;
    public float bulletSpeed;

    void Update()
    {
        if(!isLocalPlayer) {
            return;
        }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            CmdFire();
        }
    }

    // Only local player can run the command in the server
    [Command]
    void CmdFire() 
    {
        //Create bullet based on the object provided
        GameObject bullet = (GameObject)Instantiate(bulletObject, bulletSpawn.position, bulletSpawn.rotation);

        //Add velocity to bullet object
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;

        //Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        //Destroy bullet after 2 seconds
        Destroy(bullet, 2);
    }

	public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

}