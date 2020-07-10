public class DeathInfo
{

    public CharacterController source;
    public int sourceSpeed;
    public int score;

    public DeathInfo(CharacterController source, int sourceSpeed, int score)
    {
        this.source = source;
        this.sourceSpeed = sourceSpeed;
        this.score = score;
    }

}