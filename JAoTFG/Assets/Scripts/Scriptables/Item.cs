using UnityEngine;

public class Item : Entity
{

    [HideInInspector] public Player owner;

    public Rarity rarity;
    public Quality quality;
    public string description;

}