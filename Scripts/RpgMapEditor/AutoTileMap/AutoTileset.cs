#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AON.RpgMapEditor
{

    public enum eSlotAonTypeLayer : int
    {
        Ground = 0, // terrain
        Overlay = 1, // rock, tree, house
        Trigger = 2, // warps, signposts, person, script
    };

    public enum eSlotAonTypeObj : int
    {
        Default = 0, // terrain
        Warps = 1,
        Signposts = 2,
        Person = 3,
        Script = 4,
        House = 5,
        Tileset3D = 6,
        Filler3D = 7,
        Stair = 8,
        // Interior = 9,
    };

    public enum eSlotAonTypeBrush : int
    {
        High,
        Terrain,
        Filler,
        House,
        Trigger,
    };

    /// <summary>
    /// Define a subtileset inside the AutoTileset. Each subtileset is named by a letter.
    /// </summary>
    [System.Serializable]
    public class SlotAon
    {
        public string Name;
        
        public int idRef = -1;
        public Vector2 Size{
            get{
                // if(TypeObj == eSlotAonTypeObj.House)
                //     return new Vector2(5, 5);
                return new Vector2(1, 1);
            }
        }

        public eSlotAonTypeLayer TypeLayer;

        public eSlotAonTypeObj TypeObj;

        public eSlotAonTypeBrush TypeBrush;

        public bool Hidden = false;

        public Rect AtlasRecThumb;

        public int LayerDraw{
            get{
                return (int)TypeLayer;
            }
        }

        public bool IsCanCopyWhenDraw{
            get{
                if(TypeLayer == eSlotAonTypeLayer.Ground){
                    return true;
                }
                if(TypeLayer == eSlotAonTypeLayer.Overlay
                    && TypeObj != eSlotAonTypeObj.House
                    && TypeObj != eSlotAonTypeObj.Filler3D){
                    return true;
                }
                return false;
            }
        }
    }

    [System.Serializable]
    public class DrawTileAon
    {
        public enum eDrawTileAonType
        {
            Tile_2_3 = 0,
            Tile_6_3 = 1,
            Tile_1_1 = 2
        };
        public string name;
        public int xBegin;
        public int yBegin;
        public int size = 32;
        public eDrawTileAonType type = eDrawTileAonType.Tile_6_3;
    }

    [System.Serializable]
    public class DrawTileAonSet
    {
        public Texture2D TextureThumb;
        public List<DrawTileAon> DrawTileAons;
    }

    //
    [System.Serializable]
    public class DrawTile3D
    {
        // public int bitmask;
        public string prefab_name;
        public Vector3 offsetPos;
        public Vector3 offsetRotate;
    }

    [System.Serializable]
    public class DrawTile3DAon
    {
         public enum eDrawTile3DAonType
        {
            Tile_4 = 0,
            Tile_6 = 1,
            Tile_SkyBox = 2,
            Tile_Fence = 3,
        };
        public string name = "";
        public eDrawTile3DAonType type = eDrawTile3DAonType.Tile_4;
        public float scalePrefab = 1;
        public float offset = 1;
        public DrawTile3D interior;
        public DrawTile3D side;
        public DrawTile3D corner_2;
        public DrawTile3D corner_3;
        public DrawTile3D corner_1;
    }

    [System.Serializable]
    public class DrawTile3DAonSet
    {
        public int idRefSand = -1;
        // public int idRefSandF = -1;
        public List<DrawTile3DAon> DrawTile3DAons;
        public DrawTile3D WaterTile;
        public DrawTile3D WaterTileDown;
    }

    //
    public enum eOverlayArt
    {
        Bush1,
        Bush2,
        Tree1,
        Tree2,
        FountainHexdec,
        FountainOct,
        LampPostIron,
        LampRed,
        LampPostBlue,
        Bush3,
        Tree_2_1,
        Tree_2_2,
        Tree_Beach_1,
        Tree_Beach_2
    };
    [System.Serializable]
    public class OverlayAon
    {
        public eOverlayArt art;
        public Vector2 PhysicSize;
    }
    [System.Serializable]
    public class OverlayAonSet
    {
        public List<OverlayAon> OverlayAons;
    }
    /// <summary>
    /// Manage the autotileset containing all sub-tilesets named with a letter and used to draw map tiles.
    /// </summary>
    [System.Serializable]
	public class AutoTileset : ScriptableObject 
	{
        [NonSerialized]
        public string[] HouseList = {
            "Circus_EXT",
            "HealClinic_ext",
            "Theater_EXT",
            "Travel_EXT",
            "BeautySalon_LV1",
            "BigStore",
            "BK_exterior",
            "BookStore",
            "ClothStore_LV1",
            "Coffe_EXT",
            "FlowerShop_LV1",
            "Game_Shop_LV1",
            "GlovesShop_LV1",
            "Highend_res_ext",
            "Juicestore_EXT",
            "Lowend_res_ext",
            "Midend_res_ext",
            "MilkTea_EXT",
            "PerfumeShop_LV1",
            "Post_office_ex",
            "ShoppingMall_ext",
            "Sport_ext",
            "Stadium",
            "SuperMarket",
            "SwimmingPool",
            "Theatre",
            "ToyStore_LV1",
            "ToyStore_OLD",
            "Aquarium_Exterior",
            "Buffet_LV1",
            "Buffet_LV6",
            "Hotel_exterior",
            "Store_HairSalon_LV1",
            "Store_HairSalon_LV6",
            "Store_Hat_LV1",
            "Store_Hat_LV6",
            "Store_Pet_LV1",
            "Store_Pet_LV6",
            "Beach_Lighthouse",
            "Beach_GuardHouse",
            "p_apartmentsBeige",
            "p_apartmentsBlue",
            "p_apartmentsBlue2",
            "p_contestHall",
            "p_houseBeige",
            "p_houseG1Chim",
            "p_houseG2Chim",
            "p_housePurple",
            "p_library",
            "p_newTownHall",
            "p_oldTownHall",
            "p_pkmnCenter",
            "p_pkmnGym",
            "p_pkmnMart",
            "p_playerHouse",
            "p_redHouse",
            "p_restaurant",
            "p_trainStation",
            "p_trainStationHub"
        };

        [NonSerialized]
        public string[] InteriorList = {
            "Perfume_INT",
            "Circus_INT",
            "Pizza_INT",
            "SportCenter_INT",
            "Theater_INT",
            "Travel_INT",
            "BK_INT",
            "Buffet_INT",
            "Glove_INT",
            "Market_Outside",
            "Pet_Int",
            "PlayGround_INT",
            "SwimmingPool_INT",
            "Aquarium_Int",
            "BagShop_INT",
            "Bookshtore_INTERIOR",
            "Cafeteria_INTERIOR",
            "Confection_INT",
            "Dental_INT",
            "GlassShop_int",
            "Gyn_INT",
            "HatShop_Int",
            "Health_Int",
            "Hotel_Interior",
            "Juice_store_INTERIOR",
            "Library_Int",
            "Milktea_INTERIOR",
            "MiniMart_Int",
            "ShoesShop_Int",
            "Hair_Salon",
            "MobileShop_INT",
            "PO_INTERIOR",
            "ArtGal_INT",
            "Cookingworkshop_INT",
            "Flowershop_INT",
            "Gameshop_INT"
        };

        [NonSerialized]
        public string[] Filer3DList = {
            "FountainHexdec",
            "FountainOct",
            "SignWooden",
            "Umbrella",
            "Chair_Long",
            "Mailbox",
            "PhoneBox",
            "ATM",
            "Billboard",
            "Corn1",
            "Corn2",
            "Corn3",
            "Hydrant",
            "Signboard1",
            "Signboard2",
            "Signboard3",
            "Signboard4",
            "Signboard5",
            "Streetlight1A",
            "Streetlight1B",
            "Streetlight1C",
            "Streetlight1D",
            "Streetlight2",
            "Sunflower1",
            "Sunflower2",
            "Trash1",
            "Trash2",
            "Street_well1",
            "Street_well2",
            "Bench1",
            "Bridge_Wood",
            "Bridge_Car",
            "Bridge_Foot",
            "Road_Ramp",
            "Road_Ramp_Straight",
            "Road_Ramp_Pillar",
            "Brige_Wodden",
            "Chair_White",
            "Electric_Post",
            "Road_Barrier",
            "Well",
            "Mushrom1",
            "Mushrom2",
            "Mushrom3",
            "Mask1",
            "Mask2",
            "Mask3",
            "Beach_Swing",
            "Beachchair1",
            "Beachchair2",
            "Beachchair_Umbrella_1_1",
            "Beachchair_Umbrella_1_2",
            "Beach_Sand_Castle",
            "Beach_Star1",
            "Beach_Star2",
            "Beach_Surfboard1",
            "Beach_Surfboard2",
            "Beach_Mat",
            "CaveEntrance",
            "DemoPet",
            "Chest"
        };

        public string[] Filer3DListByIdRef( int idRef){
            if(idRef == 1){
                return InteriorList;
            }
            return Filer3DList;
        }
        public string GetPathFiler3D( int idRef, int idx){
            string[] data;
            string path;   
            if(idRef == 1){
                path = "interiors/";
                data = InteriorList;
            }else
            {
                path = "OBJ/";
                data = Filer3DList;    
            }
            if(idx < 0 || idx >= data.Length){
                return null;
            }
            return path + data[idx];
        }
        
        [NonSerialized]
        private string[] mNPCModel = null;
        public string[] NPCModel{
            get{
                if(mNPCModel == null || mNPCModel.Length != NPCModelList.Length){
                    mNPCModel = new string[NPCModelList.Length];
                    for(int i = 0; i < NPCModelList.Length; i++){
                        mNPCModel[i] = NPCModelList[i].name;
                    }
                }
                return mNPCModel;
            }
        }

        public enum NPCModelType{
            Humanoid,
            Legacy
        };
        
        public class NPCModel_D
        {
            public string name;
            public string patch;
            public NPCModelType type;
            public NPCModel_D(string _name, string _patch, NPCModelType _type){
                name = _name;
                patch = _patch;
                type = _type;
            }
        }

        [NonSerialized]
        public NPCModel_D[] NPCModelList = {
            // "npc_brown",
            // "npc_grey",
            // "npc_red",
            // "npc_yellow",
            new NPCModel_D("ethan", "npc_ethan", NPCModelType.Humanoid),
            new NPCModel_D("weak", "npc_weak", NPCModelType.Humanoid),
            new NPCModel_D("darwin", "npc_darwin", NPCModelType.Humanoid),
            new NPCModel_D("Boy01", "Boy/Boy01", NPCModelType.Legacy),
            new NPCModel_D("Boy02","Boy/Boy02", NPCModelType.Legacy),
            new NPCModel_D("Boy03","Boy/Boy03", NPCModelType.Legacy),
            new NPCModel_D("Boy04","Boy/Boy04", NPCModelType.Legacy),
            new NPCModel_D("Boy05","Boy/Boy05", NPCModelType.Legacy),
            new NPCModel_D("Boy06","Boy/Boy06", NPCModelType.Legacy),
            new NPCModel_D("Boy07","Boy/Boy07", NPCModelType.Legacy),
            new NPCModel_D("Boy08","Boy/Boy08", NPCModelType.Legacy),			
            new NPCModel_D("Girl01", "Girl/Girl01", NPCModelType.Legacy),
            new NPCModel_D("Girl02","Girl/Girl02", NPCModelType.Legacy),
            new NPCModel_D("Girl03","Girl/Girl03", NPCModelType.Legacy),
            new NPCModel_D("Girl04","Girl/Girl04", NPCModelType.Legacy),
            new NPCModel_D("Girl05","Girl/Girl05", NPCModelType.Legacy),
            new NPCModel_D("Girl06","Girl/Girl06", NPCModelType.Legacy),
            new NPCModel_D("Girl07","Girl/Girl07", NPCModelType.Legacy),
            new NPCModel_D("Girl08","Girl/Girl08", NPCModelType.Legacy),
            new NPCModel_D("npc_lisa","TouristFemale", NPCModelType.Humanoid),
            // new NPCModel_D("Man01", "Man/Man01", NPCModelType.Legacy),
            // new NPCModel_D("Man02","Man/Man02", NPCModelType.Legacy),
            // new NPCModel_D("Man03","Man/Man03", NPCModelType.Legacy),
            // new NPCModel_D("Man04","Man/Man04", NPCModelType.Legacy),
            // new NPCModel_D("Man05","Man/Man05", NPCModelType.Legacy),
            // new NPCModel_D("Man06","Man/Man06", NPCModelType.Legacy),
            // new NPCModel_D("Man07","Man/Man07", NPCModelType.Legacy),
            // new NPCModel_D("Man08","Man/Man08", NPCModelType.Legacy),
            // new NPCModel_D("Women01", "Women/Woman01", NPCModelType.Legacy),
            // new NPCModel_D("Women02","Women/Woman02", NPCModelType.Legacy),
            // new NPCModel_D("Women03","Women/Woman03", NPCModelType.Legacy),
            // new NPCModel_D("Women04","Women/Woman04", NPCModelType.Legacy),
            // new NPCModel_D("Women05","Women/Woman05", NPCModelType.Legacy),
            // new NPCModel_D("Women06","Women/Woman06", NPCModelType.Legacy),
            // new NPCModel_D("Women07","Women/Woman07", NPCModelType.Legacy),
            // new NPCModel_D("Women08","Women/Woman08", NPCModelType.Legacy),
        };

        public const int k_MaxTextureSize = 4096;
        public const int k_TilesPerSubTileset = 256;
		public const float PixelToUnits = 1;

		public int TileWidth = 32;

		public int TileHeight = 32;
        
        // public int TilesetSlotSize { get{ return 32 * TileWidth; } }

        public int TilePartWidth { get { return TileWidth >> 1; } }
        public int TilePartHeight { get { return TileHeight >> 1; } }

        public Texture2D TextureSlot;

        public Material Material_GroundChuck;

        public Color[] PixelsThumb(SlotAon slot){
            if(slot.TypeLayer == eSlotAonTypeLayer.Ground){
                if(slot.idRef >=0 && slot.idRef < DrawTileAonSetSelected.DrawTileAons.Count){
                    DrawTileAon d = DrawTileAonSetSelected.DrawTileAons[slot.idRef];
                    return DrawTileAonSetSelected.TextureThumb.GetPixels( d.xBegin, d.yBegin - 32, 32, 32);
                }
            }
            return TextureSlot.GetPixels( Mathf.RoundToInt(slot.AtlasRecThumb.x), Mathf.RoundToInt(slot.AtlasRecThumb.y), Mathf.RoundToInt(slot.AtlasRecThumb.width), Mathf.RoundToInt(slot.AtlasRecThumb.height));
        }
        public List<SlotAon> SlotAons;
        public bool IsExitSlot( int idSlot){
            if(idSlot >= 0 && idSlot < SlotAons.Count){
                return true;
            }
            return false;
        }
        public SlotAon GetSlot( int idSlot){
            if(idSlot >= 0 && idSlot < SlotAons.Count){
                return SlotAons[idSlot];
            }
            return null;
        }

        public int DrawSetSelected = 0;
        public List<DrawTileAonSet> DrawTileAonSets;
        public DrawTileAonSet DrawTileAonSetSelected{
            get{
                return DrawTileAonSets[DrawSetSelected];
            }
        }

        public int Draw3DSetSelected = 0;
        public List<DrawTile3DAonSet> DrawTile3DAonSets;
        public DrawTile3DAonSet DrawTile3DAonSetSelected{
            get{
                return DrawTile3DAonSets[Draw3DSetSelected];
            }
        }

        public int OverlaySetSelected = 0;
        public List<OverlayAonSet> OverlayAonSets;
        public OverlayAonSet OverlayAonSetSelected{
            get{
                return OverlayAonSets[OverlaySetSelected];
            }
        }
        
        // public List<SlotAON> GroundSlots; //Ground
        // public List<SlotAON> DecoreSlots; //Overlay
        // public List<SlotAON> HouseSlots; //House

        
        /*
        public Material AtlasMaterial;

		[SerializeField]
		private Texture2D m_atlasTexture;
        
		public Texture2D AtlasTexture
		{  
			get{return m_atlasTexture;}
			set
			{
				if( value != null && value != m_atlasTexture )
				{
                    if (value.width % TilesetSlotSize == 0 && value.height % TilesetSlotSize == 0)
					{
						m_atlasTexture = value;
						UtilsAutoTileMap.ImportTexture( m_atlasTexture );
						GenerateAutoTileData();

						if( AtlasMaterial == null )
						{
							CreateAtlasMaterial();
						}
					}
					else
					{
						m_atlasTexture = null;
                        Debug.LogError(" TilesetsAtlasTexture.set: atlas texture has a wrong size " + value.width + "x" + value.height);
					}
				}
				else
				{
					m_atlasTexture = value;
				}
			}
		}

        public void GenerateAutoTileData( )
		{
			// force to generate atlas material if it is not instantiated
			if( AtlasMaterial == null )
			{
				//Debug.LogError( "GenerateAutoTileData error: missing AtlasMaterial" );
				//return;
				CreateAtlasMaterial();
			}
			//+++ sometimes png texture loose isReadable value. Maybe a unity bug?
	#if UNITY_EDITOR
			string assetPath = AssetDatabase.GetAssetPath(AtlasTexture);
			TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter; 
			if( textureImporter != null && textureImporter.isReadable == false )
			{	// reimport texture
                Debug.LogWarning("TilesetsAtlasTexture " + assetPath + " isReadable is false. Will be re-imported to access pixels.");
				UtilsAutoTileMap.ImportTexture( AtlasTexture );
			}
	#endif
	#if UNITY_EDITOR
			EditorUtility.SetDirty( this );
	#endif

		}
        /// <summary>
        /// Create a default material for the atlas
        /// </summary>
		private void CreateAtlasMaterial()
		{
			string matPath = "";
#if UNITY_EDITOR
			matPath = System.IO.Path.GetDirectoryName( AssetDatabase.GetAssetPath( m_atlasTexture ) );
			if( !string.IsNullOrEmpty( matPath ) )
			{
				matPath += "/"+AtlasTexture.name+" atlas material.mat";
				Material matAtlas = (Material)AssetDatabase.LoadAssetAtPath( matPath, typeof(Material));
				if( matAtlas == null )
				{
                    matAtlas = new Material(Shader.Find("Sprites/Default")); //NOTE: if this material changes, remember to change also the one inside #else #endif below
					AssetDatabase.CreateAsset(matAtlas, matPath );
				}
				AtlasMaterial = matAtlas;
				EditorUtility.SetDirty( AtlasMaterial );
				AssetDatabase.SaveAssets();
            }
#else
			AtlasMaterial = new Material( Shader.Find("Sprites/Default") );
#endif

            if ( AtlasMaterial != null )
			{
				AtlasMaterial.mainTexture = AtlasTexture;
			}
			else
			{
				m_atlasTexture = null;
				Debug.LogError( " TilesetsAtlasTexture.set: there was an error creating the material asset at "+matPath );
			}
		}
         */
		
	}
}