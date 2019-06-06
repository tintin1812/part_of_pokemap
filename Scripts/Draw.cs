using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour {

	public Camera editorCam;

	//Properties
	public bool showGrid;
	public Texture gridTexture;

	public Material mainMaterial; //use this material for grid, cell and cursor
	public Color cellAddColor;
	public Color cellRemoveColor;
	public Color cursorColor;
	
	
	Material matCellAdd;
	Material matCellRemove;
	Material matCursor;
	Material matGrid;
	Color gridColorActive = new Color(255f/255f, 255f/255f, 255f/255f, 30f/255f);
	Color gridColorDeactive = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);
	
	//settings
	public string path = ""; // root is "Assets/" 
	public string fileExtension = ".xml";

	// UI Objects
	GameObject grid;
	GameObject cell;
	GameObject cursor;

	// UI painted cells arrays
	bool[,] cellMap = new bool[0, 0];
	bool[,] cellInstancedMap = new bool[0, 0];
	List<GameObject> cellList = new List<GameObject>();

	// build / map properties       
	bool paintOK = true;
	bool mapReady = false;
	int tile_width = 10;
	int tile_height = 10;
	int map_index = 1;
	bool tile_invert = false;
	int brushSize = 2;
	bool paintMask = true;
	float globalScale = 1;
	// string w = "";
	// string h = "";
	// string[] layerNames;
	//int selectedLayer = 0;
	float x = 0f;
	float z = 0f;

	//navigation properties
	bool screenPanning = false;
	Vector3 lastPosition;
	float camDistance;
	float maxDistance;
	float minDistance = 5;

	//save map properties
	string saveName = "";
		

	public void Start()
	{
		if (editorCam == null)
		{
			editorCam = GameObject.FindObjectOfType(typeof(Camera)) as Camera;
		}
		Init();
	}


	//Initialize Editor
	public void Init()
	{
		//bool array for painted cells
		cellMap = new bool[tile_width, tile_height];
		cellInstancedMap = new bool[tile_width, tile_height];

		for (int w = 0; w < tile_width; w++)
		{
			for (int h = 0; h < tile_height; h++)
			{
				cellMap[w, h] = false;
				cellInstancedMap[w, h] = false;
			}
		}


		//setup camera
		float _h = ((float)tile_height / 2);
		float _w = ((float)tile_width / (float)tile_height) * 3;
		maxDistance = _h + _w;
		camDistance = maxDistance;

		if (grid != null)
		{
			Destroy(grid.gameObject);
		}
		if (cell != null)
		{
			Destroy(cell.gameObject);
		}
		if (cursor != null)
		{
			Destroy(cursor.gameObject);
		}

		//setup editor
		//generate grid plane  
		//matGrid = new Material(Shader.Find("Particles/Additive")); -> does not work in builds, becuase shader will no be included in build
		matGrid = new Material(mainMaterial);
		if (showGrid)
		{
			matGrid.SetTexture("_MainTex", gridTexture);
			matGrid.SetColor("_TintColor", gridColorActive);
		}
		else
		{
			matGrid.SetColor("_TintColor", gridColorDeactive);
		}

		grid = GeneratePlane(tile_width, tile_height, matGrid, "grid");
		//generate cell plane and material add and material remove
		//which will be used for the cells

		//material add cell
		//matCellAdd = new Material(Shader.Find("Particles/Additive"));
		matCellAdd = new Material(mainMaterial);
		matCellAdd.SetColor("_TintColor", cellAddColor);

		////material remove cell
		//matCellRemove = new Material(Shader.Find("Particles/Additive"));
		matCellRemove = new Material(mainMaterial);
		matCellRemove.SetColor("_TintColor", cellRemoveColor);

		cell = GeneratePlane(1, 1, matCellAdd, "cell");
		//move cell
		cell.transform.position = new Vector3(-1000, -1000, -1000);

		//generate cursor
		//matCursor = new Material(Shader.Find("Particles/Additive"));
		matCursor = new Material(mainMaterial);
		matCursor.SetColor("_TintColor", cursorColor);
		cursor = GeneratePlane(2, 2, matCursor, "cursor");

		mapReady = true;

		//parent grid, cell and cursor object 
		cell.transform.parent = this.transform;
		grid.transform.parent = this.transform;
		cursor.transform.parent = this.transform;

		// this.transform.position = creator.transform.position;

	}

	//Update method is used to place cursor plane on grid and detecting if user has
	//clicked on the grid.
	void Update()
	{

		if (!mapReady || !paintOK)
			return;

		//set grid plane according to selected layer height
		if (showGrid)
		{
			grid.transform.localPosition = new Vector3(0, 1.1f + map_index, 0); // new Vector3(0, 1.1f + creator.mapIndex, 0);
		}

		Ray _ray = editorCam.ScreenPointToRay(Input.mousePosition);
		RaycastHit _hit = new RaycastHit();

		if (Physics.Raycast(_ray, out _hit, 1000)) // && _hit.transform.gameObject.tag == "grid")
		{
			//if (_hit.transform.gameObject.name == "grid")
			//{
				if (_hit.point.x - this.transform.position.x < 0)
				{
					x = (int)(_hit.point.x / 1);
				}
				else if (_hit.point.x - this.transform.position.x < tile_width - 1)
				{
					x = (int)(_hit.point.x / 1 + 1.0f);
				}

				x *= 1;

				if (_hit.point.z - this.transform.position.z < 0)
				{
					z = (int)(_hit.point.z / 1);
				}
				else if (_hit.point.z - this.transform.position.z < tile_height - 1)
				{
					z = (int)(_hit.point.z / 1 + 1.0f);
				}

				z *= 1;


				cursor.transform.position = new Vector3(x - (1), 1.2f + map_index, z - (1));

				if (Input.GetMouseButtonDown(0))
				{
					PaintCell(tile_invert);
					InstantiateCells(true);
				}
				else if (Input.GetMouseButton(0))
				{
					PaintCell(tile_invert);
					InstantiateCells(true);
				}
				else if (Input.GetMouseButtonUp(0))
				{
					BuildMap();
				}

				if (Input.GetMouseButtonDown(1))
				{
					PaintCell(!tile_invert);
					InstantiateCells(false);
				}
				else if (Input.GetMouseButton(1))
				{
					PaintCell(!tile_invert);
					InstantiateCells(false);
				}
				else if (Input.GetMouseButtonUp(1))
				{
					BuildMap();
				}


				if (Input.GetMouseButtonUp(1))
				{
					BuildMap();
				}
				else if (Input.GetMouseButtonUp(0))
				{
					BuildMap();
				}

			//}
		}



		//NAVIGATION
		//----------

		// PANNING
		// Hold middle Mouse Button to pan the screen in a direction
		if (Input.GetMouseButtonDown(2))
		{
			screenPanning = true;
			lastPosition = Input.mousePosition;
		}

		//If panning, find the angle to pan based on camera angle not screen
		if (screenPanning == true)
		{
			if (Input.GetMouseButtonUp(2))
			{
				screenPanning = false;
			}

			var delta = Input.mousePosition - lastPosition;

			editorCam.transform.Translate((-(delta.x * (0.1f / 2))), (-(delta.y * (0.1f / 2))), 0);

			//clamp panning
			editorCam.transform.localPosition = new Vector3(Mathf.Clamp(editorCam.transform.localPosition.x, 0, this.transform.position.x + tile_width), editorCam.transform.localPosition.y, Mathf.Clamp(editorCam.transform.localPosition.z, 0, this.transform.position.z + tile_height));

			lastPosition = Input.mousePosition;

		}

		//scrolling
		//---------
		camDistance -= Input.GetAxis("Mouse ScrollWheel") * 10;
		camDistance = Mathf.Clamp(camDistance, minDistance, maxDistance);

		editorCam.orthographicSize = camDistance;

		//--------------
	}

	//build map if user has clicked and automatic build is enabled 
	void BuildMap()
	{
		// ResetCells();
	}

	//paint cells so user knows where he has painted
	void InstantiateCells(bool _add)
	{
		for (int y = 0; y < tile_height; y++)
		{
			for (int x = 0; x < tile_width; x++)
			{
				if (cellMap[x, y])
				{
					if (!cellInstancedMap[x, y])
					{
						var _inst = Instantiate(cell, new Vector3(x + this.transform.position.x, 1.2f + map_index, y + this.transform.position.z), Quaternion.identity) as GameObject;
						_inst.transform.parent = this.transform;
						cellList.Add(_inst);
						cellInstancedMap[x, y] = true;

						if (_add)
						{
							_inst.GetComponent<Renderer>().material = matCellAdd;
						}
						else
						{
							_inst.GetComponent<Renderer>().material = matCellRemove;
						}
					}
				}
			}
		}
	}

	//reset painted cells
	void ResetCells()
	{
		for (int i = 0; i < cellList.Count; i++)
		{
			Destroy(cellList[i].gameObject);
		}

		cellMap = new bool[tile_width, tile_height];
		cellInstancedMap = new bool[tile_width, tile_height];
		cellList = new List<GameObject>();
	}

	//paint map, add or remove
	void PaintCell(bool _add)
	{
		if (_add)
		{
			Vector3 _gP = GetGridPosition(cursor.transform.position);


			if (_gP.x < 0)
			{
				_gP.x = -1;
			}
			if (_gP.z < 0)
			{
				_gP.z = -1;
			}

			for (int y = 0; y < brushSize; y++)
			{
				for (int x = 0; x < brushSize; x++)
				{
					if (_gP.x + x >= 0 && _gP.z + y >= 0 && _gP.x + x < this.cellMap.GetLength(0) && _gP.z + y < this.cellMap.GetLength(1))
					{
						if (!paintMask)
						{
							this.cellMap[(int)_gP.x + x, (int)_gP.z + y] = true;
						}
						else
						{
							// creator.configuration.worldMap[map_index].maskMap[(int)_gP.x + x, (int)_gP.z + y] = true;
						}


						cellMap[(int)_gP.x + x, (int)_gP.z + y] = true;

					}
				}
			}

			//creator.UpdateMap();

		}
		else
		{

			Vector3 _gP = GetGridPosition(cursor.transform.position);

			

			if (_gP.x < 0)
			{
				_gP.x = -1;
			}
			if (_gP.z < 0)
			{
				_gP.z = -1;
			}


			for (int y = 0; y < brushSize; y++)
			{
				for (int x = 0; x < brushSize; x++)
				{
					if (_gP.x + x >= 0 && _gP.z + y >= 0 && _gP.x + x < this.cellMap.GetLength(0) && _gP.z + y < this.cellMap.GetLength(1))
					{
						if (!paintMask)
						{
							this.cellMap[(int)_gP.x + x, (int)_gP.z + y] = false;
						}
						else
						{
							// creator.configuration.worldMap[map_index].maskMap[(int)_gP.x + x, (int)_gP.z + y] = false;
						}

						cellMap[(int)_gP.x + x, (int)_gP.z + y] = true;

					}
				}
			}

			//creator.UpdateMap();

		}
	}


	//return the exact grid / cell position
	Vector3 GetGridPosition(Vector3 _mousePos)
	{
		Vector3 _gridPos = new Vector3((Mathf.Floor(_mousePos.x - this.transform.position.x / 1) / globalScale), 0.05f, (Mathf.Floor(_mousePos.z - this.transform.position.z / 1) / globalScale));

		return _gridPos;
	}


	//Returns an empty plane
	public GameObject GeneratePlane(int _width, int _height, Material _mat, string _name)
	{

		Mesh _gridMesh = new Mesh();
		Vector3[] _verts = new Vector3[4];
		Vector2[] _uvs = new Vector2[4];
		int[] _tris = new int[6] { 0, 2, 1, 1, 2, 3 };
		int _count = 0;


		for (int y = 0; y < 2; y++)
		{
			for (int x = 0; x < 2; x++)
			{
				// _verts[_count] = new Vector3(0 + (x * (1 * _width)), 0, 0 + (y * (1 * _height)));
				_verts[_count] = new Vector3(0 + (x * (1 * _width)), 0 + (y * (1 * _height)), 0);
				_count++;

			}
		}

		_uvs[0] = new Vector2(0, 1);
		_uvs[1] = new Vector2(1, 1);
		_uvs[2] = new Vector2(0, 0);
		_uvs[3] = new Vector2(1, 0);


		_gridMesh.vertices = _verts;
		_gridMesh.triangles = _tris;
		_gridMesh.uv = _uvs;

		_gridMesh.RecalculateNormals();

		GameObject _newGO = new GameObject();
		_newGO.AddComponent<MeshFilter>().mesh = _gridMesh;
		_newGO.AddComponent<MeshRenderer>();
		_newGO.AddComponent<BoxCollider>();

		_newGO.transform.position = new Vector3(0, 0, 0); // new Vector3(((1 * _width) / 2), 0.0f, ((1 * _height) / 2));


		_newGO.name = _name;

		if (_mat != null)
		{
			_mat.mainTextureScale = new Vector2(_width, _height);
			_newGO.GetComponent<Renderer>().material = _mat;
			_newGO.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		}

		return _newGO;
	}


	Rect centerRect(int _width, int _height, int _xOffset, int _yOffset)
	{
		Rect _rect = new Rect((Screen.width / 2) - (_width / 2) + _xOffset, (Screen.height / 2) - (_height / 2) + _yOffset, _width, _height);
		return _rect;
	}

}
