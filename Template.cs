using CustomMapUtility;

namespace Template {
    public class EnemyTeamStageManager_Template : EnemyTeamStageManager {
        public override void OnWaveStart() {
            // This method must be called somewhere, and only once. StageManager or Passive in the OnWaveStart() method is recommended.
            // You MUST have <MapInfo>Template</MapInfo> inside your StageInfo.xml file. (Replace Template with your stage name)
            
            // When you call this method, you supply the stage name and then your map manager.
            CustomMapHandler.InitCustomMap("Template", new TemplateMapManager());
        }
        public override void OnRoundStart() {
            // Calling this will inform the game that the custom map should be the active one. Should be called in OnRoundStart() in a stage manager or passive.
            CustomMapHandler.EnforceMap();
        }
    }

    public class TemplateMapManager : CustomMapManager {
        protected internal override string[] CustomBGMs {
            get {
                // Put the file name of your BGM here, you don't need the full path.
                return new string[] {"MyBGM.mp3"};
            }
        }
    }
}