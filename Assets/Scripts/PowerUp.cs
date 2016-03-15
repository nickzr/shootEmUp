﻿using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour {
	public Vector2 rotationMinMax = new Vector2(15, 90);
	public Vector2 driftMinMax = new Vector2(0.25f, 2);
	public float lifeTime = 6f; 
	public float fadeTime = 4f; 
	public WeaponType type; 
	private GameObject cube; 
	private TextMesh letter; 
	private Vector3 rotationPerSecond;
	private float birthTime;

	void Awake() {
		cube = transform.Find("Cube").gameObject;
		letter = GetComponent<TextMesh>();

		Vector3 vel = Random.onUnitSphere; 
		vel.z = 0; 
		// Normalizing a Vector3 makes it length 1m
		vel.Normalize(); 
		vel *= Random.Range(driftMinMax.x, driftMinMax.y);
		GetComponent<Rigidbody>().velocity = vel;

		transform.rotation = Quaternion.identity;

		rotationPerSecond = new Vector3( Random.Range(rotationMinMax.x,rotationMinMax.y),
			Random.Range(rotationMinMax.x,rotationMinMax.y),
			Random.Range(rotationMinMax.x,rotationMinMax.y) );
		
		InvokeRepeating( "CheckOffscreen", 2f, 2f );
		birthTime = Time.time;
	}

	void Update () {
		cube.transform.rotation = Quaternion.Euler( rotationPerSecond*Time.time );

		float u = (Time.time - (birthTime+lifeTime)) / fadeTime;
		// For lifeTime seconds, u will be <= 0. Then it will transition to 1
		// over fadeTime seconds.
		// If u >= 1, destroy this PowerUp
		if (u >= 1) {
			Destroy( this.gameObject );
			return;
		}
		//determine the alpha value of the Cube & Letter
		if (u>0) {
			Color c = cube.GetComponent<Renderer>().material.color;
			c.a = 1f-u;
			cube.GetComponent<Renderer>().material.color = c;

			//fade letter
			c = letter.color;
			c.a = 1f - (u*0.5f);
			letter.color = c;
		}
	}

	// This SetType() differs from those on Weapon and Projectile
	public void SetType( WeaponType wt ) {
		// Grab the WeaponDefinition from Main
		WeaponDefinition def = Main.GetWeaponDefinition( wt );
		// Set the color of the Cube child
		//cube.GetComponent<Renderer>().material.color = def.color;
		//letter.color = def.color; // We could colorize the letter too
		letter.text = def.letter; // Set the letter that is shown
		type = wt; // Finally actually set the type
	}
	public void AbsorbedBy( GameObject target ) {
		// This function is called by the Hero class when a PowerUp is collected
		// We could tween into the target and shrink in size,
		// but for now, just destroy this.gameObject
		Destroy( this.gameObject );
	}
	void CheckOffscreen() {
		// If the PowerUp has drifted entirely off screen...
		if ( Utils.ScreenBoundsCheck( cube.GetComponent<Collider>().bounds,
			BoundsTest.offScreen) != Vector3.zero ) {
			// ...then destroy this GameObject
			Destroy( this.gameObject );
		}
	}
}
