﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Game : PersistableObject {

	const int saveVersion = 5;

	[SerializeField] ShapeFactory[] shapeFactories;

	[SerializeField] KeyCode createKey = KeyCode.C;
	[SerializeField] KeyCode destroyKey = KeyCode.X;
	[SerializeField] KeyCode newGameKey = KeyCode.N;
	[SerializeField] KeyCode saveKey = KeyCode.S;
	[SerializeField] KeyCode loadKey = KeyCode.L;

	[SerializeField] PersistentStorage storage;

	[SerializeField] int levelCount;

	[SerializeField] bool reseedOnLoad;

	[SerializeField] Slider creationSpeedSlider;
	[SerializeField] Slider destructionSpeedSlider;

	public float CreationSpeed { get; set; }

	public float DestructionSpeed { get; set; }

	List<Shape> shapes;

	float creationProgress, destructionProgress;

	int loadedLevelBuildIndex;

	Random.State mainRandomState;

	void OnEnable () {
		if (shapeFactories[0].FactoryId != 0) {
			for (int i = 0; i < shapeFactories.Length; i++) {
				shapeFactories[i].FactoryId = i;
			}
		}
	}

	void Start () {
		mainRandomState = Random.state;
		shapes = new List<Shape>();

		if (Application.isEditor) {
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				Scene loadedScene = SceneManager.GetSceneAt(i);
				if (loadedScene.name.Contains("Level ")) {
					SceneManager.SetActiveScene(loadedScene);
					loadedLevelBuildIndex = loadedScene.buildIndex;
					return;
				}
			}
		}

		BeginNewGame();
		StartCoroutine(LoadLevel(1));
	}

	void Update () {
		if (Input.GetKeyDown(createKey)) {
			CreateShape();
		}
		else if (Input.GetKeyDown(destroyKey)) {
			DestroyShape();
		}
		else if (Input.GetKeyDown(newGameKey)) {
			BeginNewGame();
			StartCoroutine(LoadLevel(loadedLevelBuildIndex));
		}
		else if (Input.GetKeyDown(saveKey)) {
			storage.Save(this, saveVersion);
		}
		else if (Input.GetKeyDown(loadKey)) {
			BeginNewGame();
			storage.Load(this);
		}
		else {
			for (int i = 1; i <= levelCount; i++) {
				if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
					BeginNewGame();
					StartCoroutine(LoadLevel(i));
					return;
				}
			}
		}
	}

	void FixedUpdate () {
		for (int i = 0; i < shapes.Count; i++) {
			shapes[i].GameUpdate();
		}

		creationProgress += Time.deltaTime * CreationSpeed;
		while (creationProgress >= 1f) {
			creationProgress -= 1f;
			CreateShape();
		}

		destructionProgress += Time.deltaTime * DestructionSpeed;
		while (destructionProgress >= 1f) {
			destructionProgress -= 1f;
			DestroyShape();
		}
	}

	void BeginNewGame () {
		Random.state = mainRandomState;
		int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
		mainRandomState = Random.state;
		Random.InitState(seed);

		creationSpeedSlider.value = CreationSpeed = 0;
		destructionSpeedSlider.value = DestructionSpeed = 0;

		for (int i = 0; i < shapes.Count; i++) {
			shapes[i].Recycle();
		}
		shapes.Clear();
	}

	IEnumerator LoadLevel (int levelBuildIndex) {
		enabled = false;
		if (loadedLevelBuildIndex > 0) {
			yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
		}
		yield return SceneManager.LoadSceneAsync(
			levelBuildIndex, LoadSceneMode.Additive
		);
		SceneManager.SetActiveScene(
			SceneManager.GetSceneByBuildIndex(levelBuildIndex)
		);
		loadedLevelBuildIndex = levelBuildIndex;
		enabled = true;
	}

	void CreateShape () {
		shapes.Add(GameLevel.Current.SpawnShape());
	}

	void DestroyShape () {
		if (shapes.Count > 0) {
			int index = Random.Range(0, shapes.Count);
			shapes[index].Recycle();
			int lastIndex = shapes.Count - 1;
			shapes[index] = shapes[lastIndex];
			shapes.RemoveAt(lastIndex);
		}
	}

	public override void Save (GameDataWriter writer) {
		writer.Write(shapes.Count);
		writer.Write(Random.state);
		writer.Write(CreationSpeed);
		writer.Write(creationProgress);
		writer.Write(DestructionSpeed);
		writer.Write(destructionProgress);
		writer.Write(loadedLevelBuildIndex);
		GameLevel.Current.Save(writer);
		for (int i = 0; i < shapes.Count; i++) {
			writer.Write(shapes[i].OriginFactory.FactoryId);
			writer.Write(shapes[i].ShapeId);
			writer.Write(shapes[i].MaterialId);
			shapes[i].Save(writer);
		}
	}

	public override void Load (GameDataReader reader) {
		int version = reader.Version;
		if (version > saveVersion) {
			Debug.LogError("Unsupported future save version " + version);
			return;
		}
		StartCoroutine(LoadGame(reader));
	}

	IEnumerator LoadGame (GameDataReader reader) {
		int version = reader.Version;
		int count = version <= 0 ? -version : reader.ReadInt();

		if (version >= 3) {
			Random.State state = reader.ReadRandomState();
			if (!reseedOnLoad) {
				Random.state = state;
			}
			creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
			creationProgress = reader.ReadFloat();
			destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
			destructionProgress = reader.ReadFloat();
		}

		yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
		if (version >= 3) {
			GameLevel.Current.Load(reader);
		}

		for (int i = 0; i < count; i++) {
			int factoryId = version >= 5 ? reader.ReadInt() : 0;
			int shapeId = version > 0 ? reader.ReadInt() : 0;
			int materialId = version > 0 ? reader.ReadInt() : 0;
			Shape instance = shapeFactories[factoryId].Get(shapeId, materialId);
			instance.Load(reader);
			shapes.Add(instance);
		}
	}
}