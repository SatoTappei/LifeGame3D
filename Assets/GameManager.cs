using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// ライフゲーム全体を制御する
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>セルを生成する行数</summary>
    [SerializeField] int _rowLength;
    /// <summary>セルを生成する列数</summary>
    [SerializeField] int _colLength;
    /// <summary>セルの更新速度</summary>
    [SerializeField] float _updateSpeed;
    /// <summary>最初にランダムに散らばる生きているセルの数</summary>
    [SerializeField] int _initRandomObj;
    /// <summary>物体が追加されるまでのターン数</summary>
    [Range(1,100),SerializeField] int _addTime;
    /// <summary>死んだセルを表すオブジェクト</summary>
    [SerializeField] GameObject _deadCell;
    /// <summary>生きているセルを表すオブジェクト</summary>
    [SerializeField] GameObject _aliveCell;
    /// <summary>生成したセルの親オブジェクト</summary>
    [SerializeField] Transform _cellParent;
    /// <summary>世代を表示するテキスト</summary>
    [SerializeField] TextMeshProUGUI _generationText;

    /// <summary>現在のセル全体</summary>
    bool[,] Cells { get; set; }

    void Start()
    {
        StartCoroutine(LifeGame());
    }

    void Update()
    {
        
    }

    IEnumerator LifeGame()
    {
        InitCellsRandom();
        
        // 世代をカウントする
        int geneCount = 0;

        while (true)
        {
            ClearCells();
            if(geneCount % _addTime == 0) AddObject();
            DispCells();
            yield return new WaitForSeconds(_updateSpeed);
            Cells = GetNextCells();
            geneCount++;
            _generationText.text = geneCount.ToString("000");
        }
    }

    /// <summary>セルをランダムに初期化する</summary>
    void InitCellsRandom()
    {
        // セル全体を生存していない状態(false)にする
        Cells = new bool[_rowLength, _colLength];
        for (int i = 0; i < _rowLength; i++)
            for (int j = 0; j < _colLength; j++)
                Cells[i, j] = false;

        // TODO:生存している4つのセル(四角)を2つ作る
        // 起点となるセル(左上)を決める
        // 起点となるセルは一番右の行だとはみ出てしまうので右から2番目までの値をとる
        int x = Random.Range(0, _rowLength - 1);
        // 起点となるセルは一番下の列だとはみ出てしまうので下から2番目までの値をとる
        int z = Random.Range(0, _colLength - 1);

        // 起点となるセルと隣接する3つのセルをtrueにする
        Cells[x, z] = true;
        Cells[x + 1, z] = true;
        Cells[x, z + 1] = true;
        Cells[x + 1, z + 1] = true;

        // 適当な場所を生存セルにしておく
        int count = 0;
        while (count < _initRandomObj)
        {
            int rx = Random.Range(0, _rowLength);
            int rz = Random.Range(0, _colLength);

            if (!Cells[rx, rz])
            {
                Cells[rx, rz] = true;
                count++;
            }
        }
    }

    /// <summary>セルを全部消去する</summary>
    void ClearCells()
    {
        foreach (Transform t in _cellParent)
            Destroy(t.gameObject);
    }

    /// <summary>自分と周り8つのセルの生死状態を確認する</summary>
    bool CheckCell(int x, int z)
    {
        // xが一番端の場合はそのセルを、それ以外なら横隣のセルを確認する
        int leftX = x == 0 ? 0 : x - 1;
        int rightX = x == _rowLength - 1 ? x : x + 1;
        // zが一番端の場合はそのセルを、それ以外なら縦隣のセルを確認する
        int upZ = z == 0 ? 0 : z - 1;
        int underZ = z == _colLength - 1 ? _colLength - 1 : z + 1;

        // 自分と周り8つの中で生きているセルをカウントする
        int count = 0;
        for (int i = leftX; i <= rightX; i++)
            for (int j = upZ; j <= underZ; j++)
                if (Cells[i,j]) count++;

        // このセルが生きている場合
        if (Cells[x,z])
        {
            // 生存:このセルが生きている + 隣接する生きたセルが 2 or 3
            // 過疎死:このセルが生きている + 隣接する生きたセルが 1 以下
            // 過密死:このセルが生きている + 隣接する生きたセルが 4 以上
            return 3 <= count && count <= 4;
        }
        // このセルが死んでいる場合
        else
        {
            // 誕生:このセルが死んでいる + 隣接する生きたセルが 3
            return count == 3;
        }
    }

    /// <summary>次の世代のセルを返す</summary>
    public bool[,] GetNextCells()
    {
        bool[,] next = new bool[_rowLength, _colLength];
        for (int i = 0; i < _rowLength; i++)
            for (int j = 0; j < _colLength; j++)
                next[i, j] = CheckCell(i, j);
        return next;
    }

    /// <summary>現在のセルを表示する</summary>
    public void DispCells()
    {
        for (int i = 0; i < _rowLength; i++)
        {
            for (int j = 0; j < _colLength; j++)
            {
                GameObject cell = Cells[i, j] ? _aliveCell : _deadCell;
                var obj = Instantiate(cell, new Vector3(j - _rowLength / 2, 0, i - _colLength / 2), Quaternion.identity);
                obj.transform.SetParent(_cellParent);
            }
        }
    }

    /// <summary>物体を追加する</summary>
    public void AddObject()
    {
        // ここに任意のセルを追加する処理を書く
        GenerateGlider();
    }

    // 4角からランダムでグライダーを撃ちだす
    void GenerateGlider()
    {
        int r = Random.Range(0, 4);
        (int, int)[] points = 
        {
            (1, 1),
            (1, _colLength - 2),
            (_rowLength - 2, 1),
            (_rowLength - 2, _colLength - 2),
        };
        int x = points[r].Item1;
        int z = points[r].Item2;
      
        if (r == 0 || r == 3)
        {
            int dir = r == 0 ? 1 : -1;

            Cells[x, z - dir] = true;
            Cells[x + dir, z] = true;
            Cells[x + dir, z + dir] = true;
            Cells[x, z + dir] = true;
            Cells[x - dir, z + dir] = true;
        }
        else
        {
            int dir = r == 2 ? 1 : -1;
            Cells[x, z - dir] = true;
            Cells[x - dir, z] = true;
            Cells[x - dir, z + dir] = true;
            Cells[x, z + dir] = true;
            Cells[x + dir, z + dir] = true;
        }
    }
}
