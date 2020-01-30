using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SelectionItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Image icon;

    [Header("Interactions")]
    [SerializeField] private GameObject selection;
    [SerializeField] private GameObject hover;

    private Character character;
    private CharacterSelection characterSelection;

    public void SetCharacter(Character character, CharacterSelection characterSelection)
    {

        this.characterSelection = characterSelection;
        this.character = character;
        characterName.text = character.GetName();
        icon.sprite = character.GetIcon();

    }
    public Character GetCharacter() { return character; }


    public void Deselect()
    {

        selection.SetActive(false);
    }

    public void Select()
    {

        characterSelection.DeselectAll();
        selection.SetActive(true);
        characterSelection.EnableContinue();
        NetCient.instance.SetCharacter(character);

    }

}
