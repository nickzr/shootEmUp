using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {
	public float speed = 10f; 
	public float fireRate = 0.3f; 
	public float health = 2;
	public float powerUpDropChance = 0.8f; 
	public int score = 100;
	public int showDamageForFrames; 

	[HideInInspector]public int remainingDamageFrames = 0;
	[HideInInspector]public Color[] originalColors;
	[HideInInspector]public Material[] materials;
	[HideInInspector]public Bounds bounds; 
	[HideInInspector]public Vector3 boundsCenterOffset; 

	public GameObject explosion;

	void Awake() {
		materials = Utils.GetAllMaterials( gameObject );
		originalColors = new Color[materials.Length];

		for (int i=0; i<materials.Length; i++) {
			originalColors[i] = materials[i].color;
		}

		showDamageForFrames = 5;

		InvokeRepeating( "CheckOffscreen", 0f, 2f );
	}

	void Update() {
		Move();

		//Inital amount of frames = showDamageForFrames
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
		Debug.Log ("Enemy: collision registered");

		switch (go.tag) {
		case "ProjectileHero":
			Projectile p = go.GetComponent<Projectile> ();
			Debug.Log ("Enemy: collision with projectile hero OK");

			//Destroy projectile if enemy is not inside camera view of the player yet
			bounds.center = transform.position + boundsCenterOffset;
			if (bounds.extents == Vector3.zero ||
			    Utils.ScreenBoundsCheck (bounds, BoundsTest.offScreen) != Vector3.zero) {
				Destroy (go);
				break;
			}

			ShowDamage ();
			health --;

			if (health > 0) {
				Main.S.OnEnemyHit ();
			}

			if (health <= 0) {
				Debug.Log ("Enemy: died");

				Main.S.OnEnemyDeath (this);
				Main.S.AddScore (score);

				var goExplosion = Instantiate (explosion, transform.position, transform.rotation);
				Destroy (goExplosion, 2);
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
		//How many frames to show the color
		remainingDamageFrames = showDamageForFrames;
	}

	void UnShowDamage() {
		//Called when remainingFrames = 0
		for ( int i=0; i<materials.Length; i++ ) {
			materials[i].color = originalColors[i];
		}
	}
}