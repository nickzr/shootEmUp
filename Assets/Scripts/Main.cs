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

	public GameObject[] prefabEnemies;
	public float enemySpawnPerSecond = 0.5f;
	public float enemySpawnPadding = 1.5f;
	public float enemySpawnRate;

	public GameObject[] prefabAsteroids;
	public float asteroidSpawnPerSecond = 0.5f;
	public float asteroidSpawnPadding = 2.5f;
	public float asteroidSpawnRate;

	public GameObject leftBound;
	public GameObject rightBound;

	public AudioSource audioEnemyDeath;
	public AudioSource audioEnemyHit;

	public AudioSource audioAsteroidDestroyed;

	public GUIText scoreText;
	public int score;

	public WeaponDefinition[] weaponDefinitions;
	public GameObject prefabPowerUp;
	public WeaponType[] powerUpFrequency = new WeaponType[]{
		WeaponType.blaster, WeaponType.blaster, WeaponType.spread, WeaponType.shield
	};

	private WeaponType[] activeWeaponTypes;

	void Awake () {
		S = this;

		Utils.SetCameraBounds (this.GetComponent<Camera>());

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

	public void OnEnemyHit(){
		audioEnemyHit.PlayOneShot (audioEnemyHit.clip);
	}

	public void AddScore(int newScoreValue){
		score += newScoreValue;
		UpdateScore ();
	}

	void UpdateScore(){
		scoreText.text = "Score: " + score;
	}

	public void OnEnemyDeath(Enemy e){
		audioEnemyDeath.PlayOneShot(audioEnemyDeath.clip);

		if (Random.value < e.powerUpDropChance) {
			int randomPU = Random.Range (0, powerUpFrequency.Length);
			WeaponType puType = powerUpFrequency[randomPU];

			GameObject go = Instantiate (prefabPowerUp) as GameObject;
			go.transform.parent = POWERUPS;
			PowerUp pu = go.GetComponent<PowerUp> ();
			pu.SetType (puType);
			pu.transform.position = e.transform.position;
		}
	}

	public void OnAsteroidDestroyed(){
		audioAsteroidDestroyed.PlayOneShot(audioAsteroidDestroyed.clip);
	}
}
