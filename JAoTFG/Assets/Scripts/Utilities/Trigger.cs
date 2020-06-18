/// <summary>
/// A boolean that stays true until grabbed, then turns false until set again.
/// </summary>
public class Trigger : Boolean
{
    public bool _trigger;

    public bool Value
    {
        get
        {
            var val = _trigger;
            _trigger = false;
            return val;
        }
        set { _trigger = value; }
    }

    public void SetTrigger()
    {
        Value = true;
    }
}