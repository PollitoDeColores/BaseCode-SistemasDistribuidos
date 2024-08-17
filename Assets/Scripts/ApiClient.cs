using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ApiClient : MonoBehaviour
{
    string RyMApi = "https://rickandmortyapi.com/api/character";
    [SerializeField] string apiUrl;
    [SerializeField] TextMeshProUGUI userNameText;
    [SerializeField] TextMeshProUGUI ErrorText;
    [SerializeField] TextMeshProUGUI RYMErrorText;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardsContainer;
    [SerializeField] Transform[] Positions;

    [Header ("Rick And Morty")]
    [SerializeField] TextMeshProUGUI rymName;
    [SerializeField] TextMeshProUGUI rymStatus;
    [SerializeField] TextMeshProUGUI rymSpecies;
    [SerializeField] TextMeshProUGUI rymGender;

    private int currentPlayerId = 1;
    private int Position = 0;

    void Start()
    {
        StartCoroutine(GetPlayerData(currentPlayerId));
    }

    public void SwitchPlayer()
    {
        GameObject[] _temp = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject _temp2 in _temp){
            Destroy(_temp2);
        }
        Position = 0;
        if(currentPlayerId < 5) currentPlayerId = currentPlayerId++;
        else currentPlayerId = 1;
        StartCoroutine(GetPlayerData(currentPlayerId));
    }

    private IEnumerator GetPlayerData(int playerId)
    {
        UnityWebRequest playerRequest = UnityWebRequest.Get($"{apiUrl}/players/{playerId}");
        yield return playerRequest.SendWebRequest();

        if (playerRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(playerRequest.error);
            ErrorText.text = playerRequest.error;
        }

        Player player = JsonUtility.FromJson<Player>(playerRequest.downloadHandler.text);
        userNameText.text = player.name;

        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (int cardId in player.cards)
        {
            StartCoroutine(GetCardData(cardId));
        }
    }

    private IEnumerator GetCardData(int cardId)
    {
        UnityWebRequest cardRequest = UnityWebRequest.Get($"{apiUrl}/cards/{cardId}");
        yield return cardRequest.SendWebRequest();

        if (cardRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(cardRequest.error);
            yield break;
        }

        Card card = JsonUtility.FromJson<Card>(cardRequest.downloadHandler.text);
        GameObject cardObject = Instantiate(cardPrefab, Positions[Position]);
        cardObject.GetComponentInChildren<TextMeshProUGUI>().text = card.name;
        Position++;
    }

    public void RequestRickAndMorty(){
        StartCoroutine(GetRickAndMortyData(Random.Range(0, 825)));
    }

    private IEnumerator GetRickAndMortyData(int Index)
    {
        UnityWebRequest request = UnityWebRequest.Get(RyMApi + "/" + Index);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            RYMErrorText.text = request.error;
        }

        Character character = JsonUtility.FromJson<Character>(request.downloadHandler.text);
        rymName.text = character.name;
        rymSpecies.text = character.species;
        rymGender.text = character.gender;
        rymStatus.text = character.status;

    }


    [System.Serializable]
    public class Player
    {
        public int id;
        public string name;
        public List<int> cards;
    }

    [System.Serializable]
    public class Card
    {
        public int id;
        public string name;
    }

    [System.Serializable]
    public class RickAndMortyResponse
    {
        public List<Character> results;
    }

    [System.Serializable]
    public class Character
    {
        public string name;
        public string status;
        public string species;
        public string gender;
    }
}
