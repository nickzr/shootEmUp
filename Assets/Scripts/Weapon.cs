using UnityEngine;
using System.Collections;
 
public enum WeaponType{
	none,
	blaster,
	spread,
	phaser,
	missile,
	laser,
	shield
}

[System.Serializable]
public class WeaponDefinition{
	public WeaponType type = WeaponType.none;
	public string letter;
	public Color color = Color.white;
	public GameObject projectilePrefab;
	//public Color projectileColor = Color.white;
	public float damageOnHit = 0;
	//public float continousDamage = 0; //dps (laser)
	public float delayBetweenShots = 0;
	public float velocity = 50; //speed of projectiles
}

public class Weapon : MonoBehaviour {
	public static Transform PROJECTILE_ANCHOR;
	public new AudioSource audio;
	public float increasedFireRate;

	[SerializeField]
	private WeaponType _type = WeaponType.none;
	private WeaponDefinition def;
	private GameObject _weapon;
	private float lastShot;

	void Awake(){
		_weapon = transform.Find ("bazooka").gameObject;
		increasedFireRate = 0.2f;
	}

	// Use this for initialization
	void Start () {
		SetType (_type);

		if (PROJECTILE_ANCHOR == null) {
			GameObject go = new GameObject("_Projectile_Anchor");
			PROJECTILE_ANCHOR = go.transform;
		}

		// Find the fireDelegate of the parent
		GameObject parentGO = transform.parent.gameObject;
		if (parentGO.tag == "Hero") {
			Hero.S.fireDelegate += Fire;
		}
	}

	public WeaponType type {
		get { return( _type ); }
		set { SetType( value ); }
	}

	public void SetType(WeaponType wt){
		_type = wt;
		if (type == WeaponType.none) {
			this.gameObject.SetActive (false);
			return;
		} else {
			this.gameObject.SetActive (true);
		}
		def = Main.GetWeaponDefinition (_type);
		//_weapon.GetComponent<Renderer> ().material.color = def.color;
		lastShot = 0;
	}
	
	public void Fire(){
		if (!gameObject.activeInHierarchy) {
			return;
		}
		if (Time.time - (lastShot - increasedFireRate) < def.delayBetweenShots) {
			return;
		}


		Projectile p;

		switch (type) {
		case WeaponType.blaster:
			audio.PlayOneShot (audio.clip);
			p = MakeProjectile ();
			p.GetComponent<Rigidbody> ().velocity = Vector3.up * def.velocity;
			break;

		case WeaponType.spread:
			audio.PlayOneShot (audio.clip);
			p = MakeProjectile ();
			p.GetComponent<Rigidbody> ().velocity = Vector3.up * def.velocity;
			p = MakeProjectile ();
			p.GetComponent<Rigidbody> ().velocity = new Vector3 (-0.2f, 0.9f, 0) * def.velocity;
			p = MakeProjectile ();
			p.GetComponent<Rigidbody> ().velocity = new Vector3 (0.2f, 0.9f, 0) * def.velocity;
			break;
		}
	}

	public Projectile MakeProjectile(){
		GameObject go = Instantiate(Resources.Load("ProjectileHero")) as GameObject;
		//GameObject go = Instantiate (def.projectilePrefab) as GameObject;
		if (transform.parent.gameObject.tag == "Hero") {
			go.tag = "ProjectileHero";
			go.layer = LayerMask.NameToLayer ("ProjectileHero");
		} else {
			go.tag = "ProjectileEnemy";
			go.layer = LayerMask.NameToLayer ("ProjectileEnemy");
		}

		go.transform.position = _weapon.transform.position;
		go.transform.parent = PROJECTILE_ANCHOR;
		Projectile p = go.GetComponent<Projectile> ();
		p.type = type;
		lastShot = Time.time;
		return(p);
	}
}
