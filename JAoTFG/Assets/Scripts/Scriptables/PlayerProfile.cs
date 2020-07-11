[System.Serializable]
public class PlayerProfile
{

    public string name;
    public string preferredHumanCharacterBody;
    public int level;
    public int kills;
    public int highestScore;
    public long totalScore;

    public PlayerProfile(string name)
    {
        // Capitalize first letter
        char.ToUpper(name[0]);
        this.name = name;
        this.preferredHumanCharacterBody = "Eren";
        this.level = 1;
        this.kills = 0;
        this.highestScore = 0;
        this.totalScore = 0;
    }

    public void Save()
    {
        ProfileManager.SaveExistingProfile(this);
    }

}