using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable object/Item")]
public class Item : ScriptableObject
{
    // properties of the object go here
    public GameObject prefab;
    public Sprite sprite;
    public string id;
    public string displayName;
    public ItemType type;

}

public enum ItemType
{
    Special,
    Consumable
}