﻿using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	public float speed = 10f; 
	public float fireRate = 0.3f; 
	public float health = 1;
	public int score = 100;
	public float powerUpDropChance = 1f;

	public int showDamageForFrames = 2;

	public Color[] originalColors;
	public Material[] materials;
	public int remainingDamageFrames = 0;

	public Bounds bounds; 
	public Vector3 boundsCenterOffset; 

	void Awake() {
		materials = Utils.GetAllMaterials( gameObject );
		originalColors = new Color[materials.Length];

		for (int i=0; i<materials.Length; i++) {
			originalColors[i] = materials[i].color;
		}

		InvokeRepeating( "CheckOffscreen", 0f, 2f );
	}

	void Update() {
		Move();

		if (remainingDamageFrames>0) {
			remainingDamageFrames--;
			if (remainingDamageFrames == 0) {
				UnShowDamage();
			}
		}
	}

	public virtual void Move() {
		Vector3 tempPos = pos;
		tempPos.y -= speed * Time.deltaTime;
		pos = tempPos;
	}

	public Vector3 pos {
		get {
			return(this.transform.position);
		}
		set {
			this.transform.position = value;
		}
	}

	void CheckOffscreen() {
		// If bounds are still their default value...
		if (bounds.size == Vector3.zero) {
			bounds = Utils.CombineBoundsOfChildren(this.gameObject);
			// Also find the diff between bounds.center & transform.position
			boundsCenterOffset = bounds.center - transform.position;
		}

		bounds.center = transform.position + boundsCenterOffset;

		Vector3 off = Utils.ScreenBoundsCheck( bounds, BoundsTest.offScreen );
		if ( off != Vector3.zero ) {
			if (off.y < 0) {
				Destroy( this.gameObject );
			}
		}
	}

	void OnCollisionEnter( Collision other ) {
		GameObject go = other.gameObject;

		switch (go.tag) {
		case "ProjectileHero":
			Projectile p = go.GetComponent<Projectile> ();
			// Enemies don't take damage unless they're onscreen
			// This stops the player from shooting them before they are visible
			bounds.center = transform.position + boundsCenterOffset;
			if (bounds.extents == Vector3.zero ||
			    Utils.ScreenBoundsCheck (bounds, BoundsTest.offScreen) != Vector3.zero) {
				Destroy (go);
				break;
			}
			// Hurt this Enemy
			ShowDamage ();
			Main.S.OnEnemyHit ();
			// Get the damage amount from the Projectile.type & Main.W_DEFS
			health -= Main.W_DEFS [p.type].damageOnHit;
			if (health <= 0) {
				// Destroy this Enemy
				Main.S.OnEnemyDeath (this);
				Destroy (this.gameObject);
			}
			Destroy (go);
			break;
		}
	}

	void ShowDamage() {
		foreach (Material m in materials) {
			m.color = Color.red;
		}
		remainingDamageFrames = showDamageForFrames;
	}

	void UnShowDamage() {
		for ( int i=0; i<materials.Length; i++ ) {
			materials[i].color = originalColors[i];
		}
	}
}