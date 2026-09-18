using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Elements")]
    [Range(2, 6)] //gives difficulty variable a slider in the inspector
    [SerializeField] private int difficulty = 4;


    [Header("UI Elements")]
    [SerializeField] private List<Texture2D> imageTextures;
    [SerializeField] private Transform puzzleSelectPanel;
    [SerializeField] private Image puzzleSelectPrefab;

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    private void Start()
    {
        foreach (Texture2D texture in imageTextures)
        {
            Image image = Instantiate(puzzleSelectPrefab, puzzleSelectPanel);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            image.GetComponent<Button>().onClick.AddListener(delegate { StartGame(texture); });
        }
    }

    public void StartGame(Texture2D jigsawTexture)
    {
        puzzleSelectPanel.gameObject.SetActive(false);
        pieces = new List<Transform>();
        dimensions = GetDimensions(jigsawTexture, difficulty);

        CreateJigsawPieces(jigsawTexture);
    }

    Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty)
    {
        Vector2Int dimensions = Vector2Int.zero;

        if (jigsawTexture.width < jigsawTexture.height)
        {
            dimensions.x = difficulty;
            dimensions.y = (difficulty * jigsawTexture.height) / jigsawTexture.width;
        }
        else
        {
            dimensions.x = (difficulty * jigsawTexture.width) / jigsawTexture.height;
            dimensions.y = difficulty;
        }

        return dimensions;
    }

    void CreateJigsawPieces(Texture2D jigsawTexture)
    {

    }
}
