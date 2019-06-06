using System.Collections;
using System.Collections.Generic;

namespace AON.RpgMapEditor
{
	[System.Serializable]
	public class SerializableGame{

		public List<AutoTileMapSerializeData> Map = null;
		public int MapIndex = 0;
		public Flags RawFlagWorld = new Flags();
		public SerializablePropertys Propertys = new SerializablePropertys();
		public SerializablePackages Packages = new SerializablePackages();
		public List<FlagAction.SerializableFlagAction> RawFlagAction = new List<FlagAction.SerializableFlagAction>();

		public void UpdateFrom(AutoTileMapData autoTileMapData)
		{
			RawFlagWorld = autoTileMapData.FlagWorld;
			RawFlagAction = new List<FlagAction.SerializableFlagAction>(autoTileMapData.ListFlagAction.Count);
			for (int i = 0; i < autoTileMapData.ListFlagAction.Count; i++)
			{
				var r = new FlagAction.SerializableFlagAction();
				r.FlagAction = autoTileMapData.ListFlagAction[i];
				RawFlagAction.Add(r);
			}	
		}

		public void ApplyTo(AutoTileMapData autoTileMapData){
			autoTileMapData.FlagWorld = RawFlagWorld;
			autoTileMapData.FlagWorld.CheckLockKey( AutoTileMapData.LockFlagWorld);

			autoTileMapData.ListFlagAction = new List<FlagAction>(RawFlagAction.Count);
			for (int i = 0; i < RawFlagAction.Count; i++)
			{
				autoTileMapData.ListFlagAction.Add(RawFlagAction[i].FlagAction);
			}
		}

	}
}