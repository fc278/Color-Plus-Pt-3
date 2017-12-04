using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	public GameObject cubePrefab;
	float gameLength = 60;
	Vector3 cubePosition;
	Vector3 nextCubePosition = new Vector3 (9, 11, 0);
	int gridX = 8;
	int gridY = 5;
	GameObject[ , ] grid;
	GameObject nextCube;
	float turnLength = 2;
	int numOfTurns = 0;
	Color[] randomCubeColors = { Color.red, Color.blue, Color.green, Color.magenta, Color.yellow };
	int score = 0;
	GameObject activeCube = null;
	int rainbowPoints = 10;
	int sameColorPoints = 5;
	bool gameOver = false;

	public Text ScoreText;
	public Text nextCubeText;

	// Use this for initialization
	void Start () {
		CreateGrid ();
	}

	void CreateGrid () {
		// create 8 x 5 grid
		grid = new GameObject[gridX, gridY];

		for (int x = 0; x < gridX; x++) {
			for (int y = 0; y < gridY; y++) {
				cubePosition = new Vector3 (x * 2, y * 2, 0);
				grid[x,y] = Instantiate (cubePrefab, cubePosition, Quaternion.identity);
				grid[x,y].GetComponent<CubeController> ().cubePositionX = x;
				grid[x,y].GetComponent<CubeController> ().cubePositionY = y;

			}
		}

	}

	void SpawnNextCube () {
		nextCube = Instantiate (cubePrefab, nextCubePosition, Quaternion.identity);
		nextCube.GetComponent<Renderer> ().material.color = randomCubeColors [Random.Range (0, randomCubeColors.Length)];
		nextCube.GetComponent<CubeController> ().nextCube = true;
	}

	void EndGame(bool win) {
		if (win) {
			nextCubeText.text = "You Win!";
		} else {
			nextCubeText.text = "You Lose. Try again!";
		}
		// just in case next cube still exists after game ends
		Destroy (nextCube);
		nextCube = null;

		// disable all cubes in grid
		for (int x = 0; x < gridX; x++){
			for (int y = 0; y < gridY; y++) {
				grid [x, y].GetComponent<CubeController> ().nextCube = true;
			}
		}

		gameOver = true;
	}

	GameObject PickWhiteCube (List<GameObject> whiteCubes){
		// no white cubes in row
		if (whiteCubes.Count == 0) {
			// error value
			return null;
		}

		// pick random white cube
		return whiteCubes [Random.Range (0, whiteCubes.Count)];

	}



	GameObject FindAvailableCube (int y) {
		List<GameObject> whiteCubes = new List<GameObject> ();

		// makes a list of white cubes
		for (int x = 0; x < gridX; x++) {
			if (grid [x, y].GetComponent<Renderer> ().material.color == Color.white) {
				whiteCubes.Add (grid [x, y]);
			}
		}

		return PickWhiteCube (whiteCubes);	
	}


	GameObject FindAvailableCube () {
		List<GameObject> whiteCubes = new List<GameObject> ();

		// makes a list of white cubes
		for (int y = 0; y < gridY; y++) {
			for (int x = 0; x < gridX; x++) {
				if (grid [x, y].GetComponent<Renderer> ().material.color == Color.white) {
					whiteCubes.Add (grid [x, y]);
				}
			}
		}

		return PickWhiteCube (whiteCubes);
	}


	void SetCubeColor (GameObject myCube, Color color) {
		// end game if there's no available cube in chosen row
		if (myCube == null) {
			EndGame (false);
		} else {
			// make chosen cube to be the nextCube's color
			myCube.GetComponent<Renderer> ().material.color = color;
			Destroy (nextCube);
			nextCube = null;
		}
	}


	void PlaceNextCube (int y) {
		// a cube is available if it is white
		GameObject whiteCube = FindAvailableCube (y);
		// set the white/available cube to the nextCube's color
		SetCubeColor (whiteCube, nextCube.GetComponent<Renderer>().material.color);
	}

	// set white/available cube to black if player does not presses a number key

	void AddRandomBlackCube() {
		GameObject whiteCube = FindAvailableCube ();
		// use color value that's beyond the max
		SetCubeColor (whiteCube, Color.black);
	}

	void ProcessKeyboardInput () {
		int numKeyPressed = 0;
		// tracks number input of 1-5
		if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) {
			numKeyPressed = 1;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) {
			numKeyPressed = 2;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) {
			numKeyPressed = 3;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) {
			numKeyPressed = 4;
		}
		if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) {
			numKeyPressed = 5;
		}

		// if there's still a next cube and player pressed a valid num key
		if (nextCube != null && numKeyPressed != 0) {

			// subtract 1 because grid array has a 0-based index
			// place it in specified row
			PlaceNextCube (numKeyPressed-1);
		}

	}

	public void ProcessClick (GameObject clickedCube, int x, int y, Color cubeColor, bool active) {
		
		if (cubeColor != Color.white && cubeColor != Color.black) {

			if (active) {
				// if there was an active cube, deactivate it
				clickedCube.transform.localScale /= 1.5f;
				clickedCube.GetComponent<CubeController> ().active = false;
				activeCube = null;

			} else {
				// deactivate previous active cube(s)
				if (activeCube != null) {
					activeCube.transform.localScale /= 1.5f;
					activeCube.GetComponent<CubeController> ().active = false;

				}

				// activate new clickedCube
				clickedCube.transform.localScale *= 1.5f;
				clickedCube.GetComponent<CubeController> ().active = true;
				activeCube = clickedCube;
			}

		} else if (cubeColor == Color.white) {

			// subtracts the x and y values of the active cube from the those of the clicked cube ot check the distance between them
			int distanceX = x - activeCube.GetComponent<CubeController> ().cubePositionX;
			int distanceY = y - activeCube.GetComponent<CubeController> ().cubePositionY;

			// if we are within 1 unit - includes diagonals
			// takes absolute value of the distance between the active cube and clicked cube
			if (Mathf.Abs(distanceY) <= 1 && Mathf.Abs(distanceX) <= 1) {
				// activate clicked cube by turning the clicked cube into the active cube's color
				clickedCube.GetComponent<Renderer> ().material.color = activeCube.GetComponent<Renderer> ().material.color;
				clickedCube.transform.localScale *= 1.5f;
				clickedCube.GetComponent<CubeController> ().active = true;


				// deactivate previous active cube and make it white
				activeCube.GetComponent<Renderer> ().material.color = Color.white;
				activeCube.transform.localScale /= 1.5f;
				activeCube.GetComponent<CubeController> ().active = false;


				// keep track of new active cube
				activeCube = clickedCube;
			}
		}

	}

	bool CheckRainbowPlus (int x, int y){
		Color a = grid [x, y].GetComponent<Renderer> ().material.color;
		Color b = grid [x+1, y].GetComponent<Renderer> ().material.color;
		Color c = grid [x-1, y].GetComponent<Renderer> ().material.color;
		Color d = grid [x, y+1].GetComponent<Renderer> ().material.color;
		Color e = grid [x, y-1].GetComponent<Renderer> ().material.color;


		// if any cubes are white or black, then there's no rainbow plus
		if (a == Color.white || a == Color.black ||
		    b == Color.white || b == Color.black ||
		    c == Color.white || c == Color.black ||
		    d == Color.white || d == Color.black ||
		    e == Color.white || e == Color.black) {

			return false;
		}

		// makes sure that every color is different from every other color
		if (a != b && a != c && a != d && a != e &&
		    b != c && b != d && b != e &&
		    c != d && c != e &&
		    d != e) {

			return true;
		} else {
			return false;
		}
	}

	bool CheckSameColorPlus (int x, int y){

		if (grid [x, y].GetComponent<Renderer> ().material.color != Color.white && 
			grid [x, y].GetComponent<Renderer> ().material.color != Color.black &&
			grid [x, y].GetComponent<Renderer> ().material.color == grid [x + 1, y].GetComponent<Renderer>().material.color &&
			grid [x, y].GetComponent<Renderer> ().material.color == grid [x - 1, y].GetComponent<Renderer>().material.color &&
			grid [x, y].GetComponent<Renderer> ().material.color == grid [x, y + 1].GetComponent<Renderer>().material.color &&
			grid [x, y].GetComponent<Renderer> ().material.color == grid [x, y - 1].GetComponent<Renderer>().material.color) {

			return true;

		} else {

			return false;

		}
	}

	void MakeBlackPlus (int x, int y){
		// error check - that x and y aren't on the edge of the grid
		if (x == 0|| y == 0 || x == gridX - 1 || y == gridY - 1){
			return;
		}

		grid [x, y].GetComponent<Renderer> ().material.color = Color.black;
		grid [x+1, y].GetComponent<Renderer> ().material.color = Color.black;
		grid [x-1, y].GetComponent<Renderer> ().material.color = Color.black;
		grid [x, y+1].GetComponent<Renderer> ().material.color = Color.black;
		grid [x, y-1].GetComponent<Renderer> ().material.color = Color.black;


		// if there was an active cube in the black plus
		if (activeCube != null && activeCube.GetComponent<Renderer>().material.color == Color.black) {
			// deactivate it
			activeCube.transform.localScale /= 1.5f;
			activeCube.GetComponent<CubeController> ().active = false;
			activeCube = null;
		}

	}

	void ScorePoints () {
		
		// check grid but not the edges, because the center of plus will never be on the edges
		for (int x = 1; x < gridX - 1; x++) {
			for (int y = 1; y < gridY - 1; y++) {

				// check for rainbow color plus
				if (CheckRainbowPlus (x, y)) {
					score += rainbowPoints;
					MakeBlackPlus (x, y);
				}
				// check for same color plus
				if (CheckSameColorPlus (x, y)) {
					score += sameColorPoints;
					MakeBlackPlus (x, y);
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {

		if (Time.time < gameLength) {
			ProcessKeyboardInput ();
			ScorePoints ();


			// as time increases by 2 seconds, number of turns increases by 1
			if (Time.time > turnLength * numOfTurns) {
				numOfTurns++;

				// if nextCube still exists, 
				if (nextCube != null) {
					// subtract a point
					score -= 1;
					// prevent score from going to negative range numbers
					if (score < 0) {
						score = 0;
					}
					// make a random available cube black
					AddRandomBlackCube ();
				}


				SpawnNextCube ();

			}
			// update UI score text
			ScoreText.text = "Score: " + score; 

		} else if (!gameOver) {

			// player wins if score is greater than 0
			if (score > 0) {
				EndGame (true);
			
			// player loses if score is less than 0
			} else {
				EndGame (false);
			}

		}
	}


}
