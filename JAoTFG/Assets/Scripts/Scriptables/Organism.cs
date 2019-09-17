using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Organism : Entity
{

    public Sprite portrait;
    public int health, maxHealth, stamina, maxStamina;

    private void Start()
    {
        health = maxHealth;
        stamina = maxStamina;
    }

}
