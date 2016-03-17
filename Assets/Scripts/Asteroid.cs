using UnityEngine;
using System.Collections;

public class Asteroid : MonoBehaviour {
	public float tumble;
	public int score = 50;
	public GameObject explosion;

	void Awake(){
		InvokeRepeating ("CheckOffScreen", 2f, 2f);
	}

	void Start ()
	{
		//insideUnitSphere is used to apply a random vector3 value
		GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * tumble; 
	}

	void OnCollisionEnter( Collision other ) {
		GameObject go = other.gameObject;
		Debug.Log ("Asteroid: collision registered");

		//Instantiate (explosion, transform.position, transform.rotation);

		if(go.tag == "ProjectileHero"){
			Debug.Log ("Asteroid: hit by projectile hero");
			Main.S.AddScore (score);
			Main.S.OnAsteroidDestroyed ();

			Instantiate (explosion, transform.position, transform.rotation);

			Destroy (go);
			Destroy (this.gameObject);
		}
	}

	void CheckOffScreen(){
		if (Utils.ScreenBoundsCheck (GetComponent<Collider>().bounds, 
			BoundsTest.offScreen) != Vector3.zero) {
			Destroy (this.gameObject);
		}
	}
}
