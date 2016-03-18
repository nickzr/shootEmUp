using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {
	public static Main S;
	public static Transform ENEMIES;
	public static Transform ASTEROIDS;
	public static Transform POWERUPS;
	public static Dictionary<WeaponType, WeaponDefinition> WeaponDictionary;

	public GUIText HackOnText;
	public GUIText shieldsText;
	public int shield;
	public GUIText scoreText;
	public int score;

	public GameObject[] prefabEnemies;
	[HideInInspector]public float enemySpawnPerSecond = 0.5f;
	[HideInInspector]public float enemySpawnPadding = 1.5f;
	[HideInInspector]public float enemySpawnRate;

	public GameObject[] prefabAsteroids;
	[HideInInspector]public float asteroidSpawnPerSecond = 0.5f;
	[HideInInspector]public float asteroidSpawnPadding = 2.5f;
	[HideInInspector]public float asteroidSpawnRate;

	public AudioSource audioEnemyDeath;
	public AudioSource audioEnemyHit;
	public AudioSource audioAsteroidDestroyed;
	public AudioSource audioHeroHit;
	public AudioSource audioHeroDeath;

	public WeaponDefinition[] weaponDefinitions;
	public GameObject prefabPowerUp;
	public WeaponType[] powerUpFrequency = new WeaponType[]{
		WeaponType.blaster, WeaponType.spread, WeaponType.shield
	};

	private WeaponType[] activeWeaponTypes;

	void Awake () {
		S = this;

		Utils.SetCameraBounds (this.GetComponent<Camera>());

		shield = (int) Hero.S.shieldLevel;
		UpdateShield ();
		score = 0;
		UpdateScore ();

		enemySpawnRate = 2f / enemySpawnPerSecond;
		Invoke ("SpawnEnemy", enemySpawnRate);
		asteroidSpawnRate = 1f / asteroidSpawnPerSecond;
		Invoke ("SpawnAsteroid", asteroidSpawnRate);

		if (ENEMIES == null) {
			GameObject go = new GameObject("_ENEMIES");
			ENEMIES = go.transform;
		}
		if (ASTEROIDS == null) {
			GameObject go = new GameObject("_ASTEROIDS");
			ASTEROIDS = go.transform;
		}
		if (POWERUPS == null) {
			GameObject go = new GameObject("_POWERUPS");
			POWERUPS = go.transform;
		}

		//generic dictionary with weapontype as key
		WeaponDictionary = new Dictionary<WeaponType, WeaponDefinition> ();
		foreach (WeaponDefinition def in weaponDefinitions) {
			WeaponDictionary [def.type] = def;
		}
	}
		
	void Start(){
		activeWeaponTypes = new WeaponType[weaponDefinitions.Length];
		for (int i = 0; i < weaponDefinitions.Length; i++) {
			activeWeaponTypes [i] = weaponDefinitions [i].type;
		}
	}

	void FixedUpdate(){
		setHackOnText ();
	}

	public static WeaponDefinition GetWeaponDefinition(WeaponType wt){
		//make sure the key exists in the dictionary and return it
		if (WeaponDictionary.ContainsKey (wt)) {
			return(WeaponDictionary [wt]);
		}
		return(new WeaponDefinition ());
	}

	public void SpawnEnemy () {
		int randEnemyPrefab = Random.Range (0, prefabEnemies.Length);
		GameObject go = Instantiate (prefabEnemies [randEnemyPrefab]) as GameObject;

		Vector3 pos = Vector3.zero;

		//random x position
		float xMin = Utils.camBounds.min.x+enemySpawnPadding;
		float xMax = Utils.camBounds.max.x-enemySpawnPadding;
		pos.x = Random.Range( xMin, xMax );

		pos.y = Utils.camBounds.max.y + enemySpawnPadding;

		go.transform.position = pos;
		go.transform.parent = ENEMIES;

		Invoke ("SpawnEnemy", enemySpawnRate);
	}

	public void SpawnAsteroid () {
		int randAsteroidPrefab = Random.Range (0, prefabAsteroids.Length);
		GameObject go = Instantiate (prefabAsteroids [randAsteroidPrefab]) as GameObject;

		Vector3 pos = Vector3.zero;

		float xMin = Utils.camBounds.min.x+asteroidSpawnPadding;
		float xMax = Utils.camBounds.max.x-asteroidSpawnPadding;
		pos.x = Random.Range( xMin, xMax );

		pos.y = Utils.camBounds.max.y + asteroidSpawnPadding;

		go.transform.position = pos;
		go.transform.parent = ASTEROIDS;
		Invoke ("SpawnAsteroid", asteroidSpawnRate);
	}

	public void DelayedRestart(float delay){
		Invoke ("Restart", delay);
	}

	public void Restart(){
		SceneManager.LoadScene ("Scene_0");
	}

	public void AddScore(int newScoreValue){
		score += newScoreValue;
		UpdateScore ();
	}

	void setHackOnText(){
		if (Hero.S.weaponDuration > 0 && Hero.S.weapon.type == WeaponType.spread) {
			HackOnText.text = "HACK ACTIVATED FOR " + (Hero.S.weaponDuration / 100) + " SECONDS";
		} else {
			HackOnText.text = "";
		}
	}

	void UpdateScore(){
		scoreText.text = "Score: " + score;
	}

	void UpdateShield(){
		shieldsText.text = "Shields: " + shield;
	}

	public void AddShield(int newShieldValue){
		shield += newShieldValue;
		UpdateShield ();
	}

	public void SubstractShield(int newShieldValue){
		shield -= newShieldValue;
		UpdateShield ();
	}

	public void OnEnemyHit(){
		audioEnemyHit.PlayOneShot (audioEnemyHit.clip);
	}

	public void OnEnemyDeath(Enemy e){
		audioEnemyDeath.PlayOneShot(audioEnemyDeath.clip);

		if (Random.value < e.powerUpDropChance) {
			int randomPU = Random.Range (0, powerUpFrequency.Length - 1);
			WeaponType puType = powerUpFrequency [randomPU];

			GameObject go = Instantiate (prefabPowerUp) as GameObject;
			go.transform.parent = POWERUPS;
			PowerUp pu = go.GetComponent<PowerUp> ();
			pu.SetType (puType);
			pu.transform.position = e.transform.position;
		}
	}

	public void OnHeroHit(){
		audioHeroHit.PlayOneShot (audioHeroHit.clip);
	}

	public void OnHeroDeath(){
		audioHeroDeath.PlayOneShot(audioHeroDeath.clip);
	}

	public void OnAsteroidDestroyed(){
		audioAsteroidDestroyed.PlayOneShot(audioAsteroidDestroyed.clip);
	}
}
