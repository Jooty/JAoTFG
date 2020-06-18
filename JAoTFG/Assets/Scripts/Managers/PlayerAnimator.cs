public class PlayerAnimator : CharacterAnimator
{
    // locals
    private PlayerController playerController;

    new private void Awake()
    {
        this.playerController = GetComponent<PlayerController>();

        base.Awake();
    }

    new private void Update()
    {
        base.animator.SetBool("usingManGear", playerController.usingManGear);
        base.animator.SetBool("isHooked", playerController.hooks.Count > 0);
        base.animator.SetBool("isSliding", playerController.isSliding);

        base.Update();
    }
}