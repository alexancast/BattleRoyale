using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class CharacterSelection : MonoBehaviour
{

    [SerializeField] private Character[] characters;
    [SerializeField] private GameObject heroPanel;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private Button continueButton;

    private SelectionItem[] selectionItems;

    public void Start()
    {
        selectionItems = new SelectionItem[characters.Length];
        continueButton.interactable = false;

        for(int i = 0; i < characters.Length; i++)
        {
            SelectionItem selectionItem = Instantiate(characterItemPrefab, heroPanel.transform).GetComponent<SelectionItem>();
            selectionItems[i] = selectionItem;
            selectionItem.SetCharacter(characters[i], this);
        }

        NetCient.instance.SetCharacter(characters[0]);
        StartCoroutine(CountdownToStart());
    }

    public IEnumerator CountdownToStart()
    {
        yield return new WaitForSeconds(20);
        SceneManager.LoadScene("SampleScene");
    }

    public void DeselectAll()
    {
        foreach (SelectionItem item in selectionItems)
        {
            item.Deselect();
        }

        continueButton.interactable = false;

    }

    public void EnableContinue()
    {
        continueButton.interactable = true;

    }

}
