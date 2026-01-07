using UnityEngine;

public class InfiniteBackgroundAuto : MonoBehaviour
{
    public static InfiniteBackgroundAuto Instance { get; private set; }

    [Header("Settings")]
    public float scrollSpeed = 2f;
    public int numberOfTiles = 3;
    public float startOffset = 0f;
    public float speedIncrease = 0.5f;   
    public float increaseInterval = 15f; 

    private Transform[] tiles;
    private int leftIndex;
    private int rightIndex;
    private float tileWidth;

    private float timer; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        tiles = new Transform[numberOfTiles];
        for (int i = 0; i < numberOfTiles; i++)
        {
            tiles[i] = transform.GetChild(i);
        }

        SpriteRenderer sr = tiles[0].GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            tileWidth = sr.bounds.size.x;
        }
        else
        {
            Debug.LogError("El primer tile no tiene SpriteRenderer, no se puede calcular el ancho.");
            tileWidth = 1f;
        }

        leftIndex = 0;
        rightIndex = tiles.Length - 1;

        float startX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + startOffset;

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].position = new Vector3(startX + i * tileWidth, tiles[i].position.y, tiles[i].position.z);
        }

        timer = increaseInterval; 
    }

    void Update()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].position += Vector3.left * scrollSpeed * Time.deltaTime;
        }

        float camLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

        if (tiles[leftIndex].position.x < camLeft - tileWidth)
        {
            ScrollRight();
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            scrollSpeed += speedIncrease; 
            timer = increaseInterval;     
        }
    }

    void ScrollRight()
    {
        tiles[leftIndex].position = new Vector3(
            tiles[rightIndex].position.x + tileWidth,
            tiles[leftIndex].position.y,
            tiles[leftIndex].position.z
        );

        rightIndex = leftIndex;
        leftIndex++;
        if (leftIndex == tiles.Length)
        {
            leftIndex = 0;
        }
    }
}
