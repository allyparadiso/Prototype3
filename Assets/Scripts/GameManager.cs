using JetBrains.Annotations;
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
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;


    [Header("UI Elements")]
    [SerializeField] private List<Texture2D> imageTextures;
    [SerializeField] private Transform puzzleSelectPanel;
    [SerializeField] private RawImage puzzleSelectPrefab;
    [SerializeField] private GameObject playAgainButton;

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    private Transform draggingPiece = null;
    private Vector3 offset;

    private int piecesCorrect;

    private void Start()
    {
        foreach (Texture2D texture in imageTextures)
        {
            RawImage rawImage = Instantiate(puzzleSelectPrefab, puzzleSelectPanel);
            rawImage.texture = texture;
            rawImage.GetComponent<Button>().onClick.AddListener(delegate { StartGame(texture); });
        }
    }

    public void StartGame(Texture2D jigsawTexture)
    {
        puzzleSelectPanel.gameObject.SetActive(false);
        pieces = new List<Transform>();
        dimensions = GetDimensions(jigsawTexture, difficulty);

        CreateJigsawPieces(jigsawTexture);

        ScatterPieces();

        UpdateBorder();

        piecesCorrect = 0;
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
        height = 1f / dimensions.y;
        float aspect = (float)jigsawTexture.width / jigsawTexture.height;
        width = aspect / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                // creates pieces in the right place and of the right size
                Transform piece = Instantiate(piecePrefab, gameHolder);
                piece.localPosition = new Vector3(
                    (-width * dimensions.x / 2) + (width * col) + (width / 2),
                    (-height * dimensions.y / 2) + (height * row) +(height / 2),
                    -1);
                piece.localScale = new Vector3(width, height, 1f);

                piece.name = $"Piece {(row * dimensions.x) + col}";
                pieces.Add(piece);

                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;

                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(width1 * col, height1 * row);
                uv[1] = new Vector2(width1 * (col + 1), height1 * row); 
                uv[2] = new Vector2(width1 * col, height1 * (row + 1));
                uv[3] = new Vector2(width1 * (col + 1), height1 * (row + 1));

                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;

                piece.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jigsawTexture);
            }
        }
    }

    private void ScatterPieces()
    {
        float orthoHeight = Camera.main.orthographicSize;
        float screenAspect = (float)Screen.width / Screen.height;
        float orthoWidth = (screenAspect * orthoHeight);

        float pieceWidth = width * gameHolder.localScale.x;
        float pieceHeight = height * gameHolder.localScale.y;

        orthoHeight -= pieceHeight;
        orthoWidth -= pieceWidth;

        foreach (Transform piece in pieces)
        {
            float x = Random.Range(-orthoWidth, orthoWidth);
            float y = Random.Range(-orthoHeight, orthoHeight);
            piece.position = new Vector3(x, y, -1);
        }
    }

    private void UpdateBorder() // border surrounding the edges of the puzzle
    {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();

        float halfWidth = (width * dimensions.x) / 2;
        float halfHeight = (height * dimensions.y) / 2;

        float borderZ = 0f;

        lineRenderer.SetPosition(0, new Vector3(-halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(1, new Vector3(halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(2, new Vector3(halfWidth, -halfHeight, borderZ));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, borderZ));

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.enabled = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                draggingPiece = hit.transform;
                offset = draggingPiece.position - hit.point;
                offset += Vector3.back;
            }
        }

        if (draggingPiece && Input.GetMouseButtonUp(0))
        {
            SnapAndDisableIfCorrect();
            draggingPiece.position += Vector3.forward;
            draggingPiece = null;
        }

        if (draggingPiece)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //newPosition.z = draggingPiece.position.z;
            newPosition += offset;
            draggingPiece.position = newPosition;
        }
    }

    private void SnapAndDisableIfCorrect()
    {
        int pieceIndex = pieces.IndexOf(draggingPiece); // check the index of piece to know if it's correct

        // coords of the piece
        int col = pieceIndex % dimensions.x; 
        int row = pieceIndex / dimensions.x;

        Vector2 targetPosition = new((-width * dimensions.x / 2) + (width * col) + (width / 2), (-height * dimensions.y / 2) + (height * row) + (height / 2)); // target pos coordinates

        if (Vector2.Distance(draggingPiece.localPosition, targetPosition) < (width / 2)) // check if it's in right location
        {
            draggingPiece.localPosition = targetPosition;

            draggingPiece.GetComponent<MeshCollider>().enabled = false; // disable collider so it can't be clicked on

            piecesCorrect++;
            if (piecesCorrect == pieces.Count)
            {
                playAgainButton.SetActive(true);
            }
        }
    }

    public void RestartGame()
    {
        foreach (Transform piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        pieces.Clear();

        gameHolder.GetComponent<LineRenderer>().enabled = false;

        playAgainButton.SetActive(false);
        puzzleSelectPanel.gameObject.SetActive(true);
    }
}
