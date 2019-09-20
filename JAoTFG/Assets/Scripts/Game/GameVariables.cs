public static class GameVariables
{

    // DEBUG
    public const bool DEBUG_DRAW_GROUND_CHECK_RAY = true;

    // HERO
    public const float HERO_WAIT_FOR_GROUND_CHECK_AFTER_JUMP = .5f;
    public const float HERO_WAIT_TO_ROTATE_AFTER_BURST_JUMP = 1f;
    public const float HERO_STRIKE_RAY_LENGTH = 5f;
    public const float HERO_MAX_SPEED = 25f;
    public const float HERO_AIR_ROTATE_SPEED = 3f;

    // MANEUVER GEAR
    public const bool MG_RETRACT_ON_GAS = true;
    public const float MG_HOOK_RANGE = 50f;
    public const float MG_HOOK_MAX_RUNAWAY_RANGE = MG_HOOK_RANGE * 1.5f;
    public const float MG_GAS_REEL_SPEED_MULTIPLIER = .935f;

    // TITAN
    public const int TITAN_DISSIPATE_TIMER = 30;

    // GAME
    public const float FIELD_OF_VIEW = 80;

}
