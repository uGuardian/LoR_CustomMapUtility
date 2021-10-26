using CustomMapUtility;

namespace Template {
    public class TemplateStageManager : EnemyTeamStageManager {
        public override void OnWaveStart() {
            // This method must be called somewhere, and only once. StageManager or Passive in the OnWaveStart() method is reccomended.
            // You MUST have <MapInfo>Template</MapInfo> inside your StageInfo.xml file. (Replace Template with your stage name)
            // When you call this method, you supply the stage name and then your map manager.
            CustomMapHandler.InitCustomMap("Template", new TemplateMapManager());
        }
        public override void OnRoundStart() {
            // Calling this will inform the game that the custom map should be the active one. Should be called in OnRoundStart() in a stage manager or passive.
            CustomMapHandler.EnforceMap();
        }
    }

    // You probably only want to use one manager.
    public class TemplateMapManager : CustomMapManager {
        protected internal override string[] CustomBGMs {
            get {
                return new string[] {"MyBGM"};
            }
        }
    }
    // ADVANCED: This is if you want to use the base functions of the CreatureMapManager rather than just the normal manager.
    public class TemplateCreatureMapManager : CustomCreatureMapManager {
        protected internal override string[] CustomBGMs {
            get {
                return new string[] {"MyBGM"};
            }
        }
    }
}