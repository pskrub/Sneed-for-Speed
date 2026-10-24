using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Match-3 board built from UI tiles. Click a tile, then click a neighbour to swap.
// Matches change the snail's speed. Salt slowly spreads if you leave it alone.
public class Board : MonoBehaviour
{
    [System.Serializable]
    public class TileKind
    {
        public string name;
        public Color color = Color.white;
        [Tooltip("Optional. If set, used instead of the flat color.")]
        public Sprite sprite;
        [Tooltip("Speed change for a match of 3. Bigger matches give proportionally more.")]
        public float speedChange = 1f;
        public float spawnWeight = 1f;
        [Tooltip("Tick this for salt: it spreads over the board over time.")]
        public bool spreads;
    }

    public SnailSpeedCalc snail;

    [Header("Grid")]
    public int width = 7;
    public int height = 7;
    public float tileSize = 120f;
    public float spacing = 8f;

    [Header("Tiles")]
    public TileKind[] kinds =
    {
        new TileKind { name = "Strawberry", color = new Color(0.90f, 0.20f, 0.25f), speedChange = 1.5f, spawnWeight = 1f },
        new TileKind { name = "Leaf",       color = new Color(0.30f, 0.75f, 0.30f), speedChange = 0.8f, spawnWeight = 1.2f },
        new TileKind { name = "Dewdrop",    color = new Color(0.30f, 0.60f, 1.00f), speedChange = 0.8f, spawnWeight = 1.2f },
        new TileKind { name = "Mushroom",   color = new Color(0.65f, 0.45f, 0.30f), speedChange = 0.8f, spawnWeight = 1.2f },
        new TileKind { name = "Salt",       color = new Color(0.92f, 0.92f, 0.92f), speedChange = -2f, spawnWeight = 0.6f, spreads = true },
    };

    [Header("Feel")]
    public float popTime = 0.15f;
    public float growTime = 0.12f;
    [Tooltip("Extra bonus on good matches for each cascade step (0.25 = +25%).")]
    public float chainBonus = 0.25f;
    [Tooltip("Seconds between salt spreading. 0 = off.")]
    public float spreadInterval = 6f;

    const int Empty = -1;

    int[,] grid;
    Image[,] tiles;
    Vector2Int? selected;
    bool busy;
    float spreadTimer;

    bool Stopped => snail != null && snail.IsDead;

    void Start()
    {
        grid = new int[width, height];
        tiles = new Image[width, height];
        BuildTiles();
        FillWithoutMatches();
        RefreshAll();
    }

    void Update()
    {
        if (spreadInterval <= 0f || busy || Stopped) return;

        spreadTimer += Time.deltaTime;
        if (spreadTimer >= spreadInterval)
        {
            spreadTimer = 0f;
            SpreadSalt();
        }
    }

    // ---------- Setup ----------

    void BuildTiles()
    {
        var rt = (RectTransform)transform;
        float step = tileSize + spacing;
        rt.sizeDelta = new Vector2(width * step - spacing, height * step - spacing);

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            var go = new GameObject($"Tile {x},{y}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);

            var r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = Vector2.zero; // bottom-left of the board
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = Vector2.one * tileSize;
            r.anchoredPosition = new Vector2(x * step + tileSize / 2f, y * step + tileSize / 2f);

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            int cx = x, cy = y;
            button.onClick.AddListener(() => OnTileClicked(cx, cy));

            tiles[x, y] = go.GetComponent<Image>();
        }
    }

    void FillWithoutMatches()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            int k, tries = 0;
            do { k = RandomKind(); tries++; }
            while (tries < 50 && MakesMatchAt(x, y, k));
            grid[x, y] = k;
        }
    }

    bool MakesMatchAt(int x, int y, int k)
    {
        bool h = x >= 2 && grid[x - 1, y] == k && grid[x - 2, y] == k;
        bool v = y >= 2 && grid[x, y - 1] == k && grid[x, y - 2] == k;
        return h || v;
    }

    int RandomKind()
    {
        float total = 0f;
        foreach (var k in kinds) total += k.spawnWeight;
        float r = Random.value * total;
        for (int i = 0; i < kinds.Length; i++)
        {
            r -= kinds[i].spawnWeight;
            if (r <= 0f) return i;
        }
        return kinds.Length - 1;
    }

    // ---------- Input ----------

    void OnTileClicked(int x, int y)
    {
        if (busy || Stopped) return;

        var clicked = new Vector2Int(x, y);

        if (selected == null)
        {
            Select(clicked);
            return;
        }

        var s = selected.Value;
        Deselect();

        if (s == clicked) return;

        bool neighbours = Mathf.Abs(s.x - x) + Mathf.Abs(s.y - y) == 1;
        if (neighbours)
            StartCoroutine(TrySwap(s, clicked));
        else
            Select(clicked);
    }

    void Select(Vector2Int p)
    {
        selected = p;
        tiles[p.x, p.y].transform.localScale = Vector3.one * 1.15f;
    }

    void Deselect()
    {
        if (selected is Vector2Int p)
            tiles[p.x, p.y].transform.localScale = Vector3.one;
        selected = null;
    }

    // ---------- Core loop ----------

    IEnumerator TrySwap(Vector2Int a, Vector2Int b)
    {
        busy = true;
        Swap(a, b);
        RefreshAll();
        yield return new WaitForSeconds(0.1f);

        var matches = FindMatches();
        if (matches.Count == 0)
        {
            Swap(a, b); // no match: swap back
            RefreshAll();
            busy = false;
            yield break;
        }

        yield return Resolve(matches);
    }

    IEnumerator Resolve(HashSet<Vector2Int> matches)
    {
        busy = true;
        int chain = 1;

        while (matches.Count > 0)
        {
            ApplyMatchEffects(matches, chain);

            yield return ScaleCells(new List<Vector2Int>(matches), 1f, 0f, popTime);
            foreach (var p in matches) grid[p.x, p.y] = Empty;

            Collapse();
            var refilled = Refill();
            RefreshAll();
            foreach (var p in refilled) tiles[p.x, p.y].transform.localScale = Vector3.zero;
            yield return ScaleCells(refilled, 0f, 1f, growTime);

            matches = FindMatches();
            chain++;
        }

        if (!HasPossibleMove())
        {
            yield return new WaitForSeconds(0.3f);
            Shuffle();
        }

        busy = false;
    }

    void ApplyMatchEffects(HashSet<Vector2Int> matched, int chain)
    {
        if (snail == null) return;

        var counts = new int[kinds.Length];
        foreach (var p in matched) counts[grid[p.x, p.y]]++;

        float total = 0f;
        for (int i = 0; i < kinds.Length; i++)
        {
            if (counts[i] == 0) continue;
            float change = kinds[i].speedChange * counts[i] / 3f;
            if (change > 0f) change *= 1f + chainBonus * (chain - 1);
            total += change;
        }
        snail.ChangeSpeed(total);
    }

    HashSet<Vector2Int> FindMatches()
    {
        var result = new HashSet<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            int run = 1;
            for (int x = 1; x <= width; x++)
            {
                bool same = x < width && grid[x, y] != Empty && grid[x, y] == grid[x - 1, y];
                if (same) { run++; continue; }
                if (run >= 3 && grid[x - 1, y] != Empty)
                    for (int k = 1; k <= run; k++) result.Add(new Vector2Int(x - k, y));
                run = 1;
            }
        }

        for (int x = 0; x < width; x++)
        {
            int run = 1;
            for (int y = 1; y <= height; y++)
            {
                bool same = y < height && grid[x, y] != Empty && grid[x, y] == grid[x, y - 1];
                if (same) { run++; continue; }
                if (run >= 3 && grid[x, y - 1] != Empty)
                    for (int k = 1; k <= run; k++) result.Add(new Vector2Int(x, y - k));
                run = 1;
            }
        }

        return result;
    }

    void Collapse()
    {
        for (int x = 0; x < width; x++)
        {
            int write = 0;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == Empty) continue;
                if (write != y)
                {
                    grid[x, write] = grid[x, y];
                    grid[x, y] = Empty;
                }
                write++;
            }
        }
    }

    List<Vector2Int> Refill()
    {
        var filled = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] != Empty) continue;
            grid[x, y] = RandomKind();
            filled.Add(new Vector2Int(x, y));
        }
        return filled;
    }

    bool HasPossibleMove()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            var a = new Vector2Int(x, y);
            foreach (var b in new[] { new Vector2Int(x + 1, y), new Vector2Int(x, y + 1) })
            {
                if (b.x >= width || b.y >= height) continue;
                Swap(a, b);
                bool found = FindMatches().Count > 0;
                Swap(a, b);
                if (found) return true;
            }
        }
        return false;
    }

    void Shuffle()
    {
        int tries = 0;
        do { FillWithoutMatches(); tries++; }
        while (!HasPossibleMove() && tries < 100);
        RefreshAll();
    }

    void Swap(Vector2Int a, Vector2Int b)
    {
        (grid[a.x, a.y], grid[b.x, b.y]) = (grid[b.x, b.y], grid[a.x, a.y]);
    }

    // ---------- Salt ----------

    int SaltIndex()
    {
        for (int i = 0; i < kinds.Length; i++)
            if (kinds[i].spreads) return i;
        return -1;
    }

    void SpreadSalt()
    {
        int salt = SaltIndex();
        if (salt < 0) return;

        // Prefer spreading next to existing salt.
        var candidates = new List<Vector2Int>();
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] != salt) continue;
            foreach (var d in dirs)
            {
                var n = new Vector2Int(x + d.x, y + d.y);
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) continue;
                if (grid[n.x, n.y] != salt) candidates.Add(n);
            }
        }

        Vector2Int target = candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : new Vector2Int(Random.Range(0, width), Random.Range(0, height));

        grid[target.x, target.y] = salt;
        RefreshTile(target.x, target.y);

        // Spreading salt can line up 3 salts by itself. Ignoring salt has a cost!
        var matches = FindMatches();
        if (matches.Count > 0)
        {
            Deselect();
            StartCoroutine(Resolve(matches));
        }
    }

    // ---------- Visuals ----------

    void RefreshAll()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            RefreshTile(x, y);
    }

    void RefreshTile(int x, int y)
    {
        var img = tiles[x, y];
        int k = grid[x, y];
        if (k == Empty) { img.enabled = false; return; }

        img.enabled = true;
        img.transform.localScale = Vector3.one;
        var kind = kinds[k];
        img.sprite = kind.sprite;
        img.color = kind.sprite != null ? Color.white : kind.color;
    }

    IEnumerator ScaleCells(List<Vector2Int> cells, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(from, to, t / duration);
            foreach (var c in cells) tiles[c.x, c.y].transform.localScale = Vector3.one * s;
            yield return null;
        }
        foreach (var c in cells) tiles[c.x, c.y].transform.localScale = Vector3.one * to;
    }
}
