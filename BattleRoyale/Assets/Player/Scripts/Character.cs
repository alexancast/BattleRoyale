using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character")]
public class Character : ScriptableObject
{

    [SerializeField] private string characterName;
    [SerializeField] private Sprite characterIcon;
    [SerializeField][TextArea] string characterDescription;
    [SerializeField] private GameObject characterPrefab;


    public string GetName() { return characterName; }
    public Sprite GetIcon() { return characterIcon; }
    public string GetDescription() { return characterDescription; }
    public GameObject GetPrefab() { return characterPrefab; }


}
