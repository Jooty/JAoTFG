using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanBodyHitbox : MonoBehaviour
{

    public TitanBodyHitboxType hitboxType;

    private bool canBeHit = true;

    private TitanController titanController;

    private void Awake()
    {
        titanController = transform.FindParentWithTag("Titan").GetComponent<TitanController>();
    }

    public void Hit()
    {
        if (!canBeHit) return;

        StartCoroutine(RecoverTimer());
        titanController.HitboxHitEvent(hitboxType);
    }

    private IEnumerator RecoverTimer()
    {
        canBeHit = false;
        yield return new WaitForSeconds(getRecoveryTime());
        canBeHit = true;
    }

    private float getRecoveryTime()
    {
        switch (hitboxType)
        {
            case TitanBodyHitboxType.ankle:
                return titanController.ankleSlashRecoveryTime;
            case TitanBodyHitboxType.eyes:
                return titanController.eyeSlashRecoveryTime;
            case TitanBodyHitboxType.nape:
                return int.MaxValue;
            default:
                Debug.LogWarning($"{gameObject.name} does not have a hitbox type!");
                return 0f;
        }
    }

}
