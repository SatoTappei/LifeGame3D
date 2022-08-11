using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// ���C�t�Q�[���S�̂𐧌䂷��
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>�Z���𐶐�����s��</summary>
    [SerializeField] int _rowLength;
    /// <summary>�Z���𐶐������</summary>
    [SerializeField] int _colLength;
    /// <summary>�Z���̍X�V���x</summary>
    [SerializeField] float _updateSpeed;
    /// <summary>�ŏ��Ƀ����_���ɎU��΂鐶���Ă���Z���̐�</summary>
    [SerializeField] int _initRandomObj;
    /// <summary>���̂��ǉ������܂ł̃^�[����</summary>
    [Range(1,100),SerializeField] int _addTime;
    /// <summary>���񂾃Z����\���I�u�W�F�N�g</summary>
    [SerializeField] GameObject _deadCell;
    /// <summary>�����Ă���Z����\���I�u�W�F�N�g</summary>
    [SerializeField] GameObject _aliveCell;
    /// <summary>���������Z���̐e�I�u�W�F�N�g</summary>
    [SerializeField] Transform _cellParent;
    /// <summary>�����\������e�L�X�g</summary>
    [SerializeField] TextMeshProUGUI _generationText;

    /// <summary>���݂̃Z���S��</summary>
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
        
        // ������J�E���g����
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

    /// <summary>�Z���������_���ɏ���������</summary>
    void InitCellsRandom()
    {
        // �Z���S�̂𐶑����Ă��Ȃ����(false)�ɂ���
        Cells = new bool[_rowLength, _colLength];
        for (int i = 0; i < _rowLength; i++)
            for (int j = 0; j < _colLength; j++)
                Cells[i, j] = false;

        // TODO:�������Ă���4�̃Z��(�l�p)��2���
        // �N�_�ƂȂ�Z��(����)�����߂�
        // �N�_�ƂȂ�Z���͈�ԉE�̍s���Ƃ͂ݏo�Ă��܂��̂ŉE����2�Ԗڂ܂ł̒l���Ƃ�
        int x = Random.Range(0, _rowLength - 1);
        // �N�_�ƂȂ�Z���͈�ԉ��̗񂾂Ƃ͂ݏo�Ă��܂��̂ŉ�����2�Ԗڂ܂ł̒l���Ƃ�
        int z = Random.Range(0, _colLength - 1);

        // �N�_�ƂȂ�Z���Ɨאڂ���3�̃Z����true�ɂ���
        Cells[x, z] = true;
        Cells[x + 1, z] = true;
        Cells[x, z + 1] = true;
        Cells[x + 1, z + 1] = true;

        // �K���ȏꏊ�𐶑��Z���ɂ��Ă���
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

    /// <summary>�Z����S����������</summary>
    void ClearCells()
    {
        foreach (Transform t in _cellParent)
            Destroy(t.gameObject);
    }

    /// <summary>�����Ǝ���8�̃Z���̐�����Ԃ��m�F����</summary>
    bool CheckCell(int x, int z)
    {
        // x����Ԓ[�̏ꍇ�͂��̃Z�����A����ȊO�Ȃ牡�ׂ̃Z�����m�F����
        int leftX = x == 0 ? 0 : x - 1;
        int rightX = x == _rowLength - 1 ? x : x + 1;
        // z����Ԓ[�̏ꍇ�͂��̃Z�����A����ȊO�Ȃ�c�ׂ̃Z�����m�F����
        int upZ = z == 0 ? 0 : z - 1;
        int underZ = z == _colLength - 1 ? _colLength - 1 : z + 1;

        // �����Ǝ���8�̒��Ő����Ă���Z�����J�E���g����
        int count = 0;
        for (int i = leftX; i <= rightX; i++)
            for (int j = upZ; j <= underZ; j++)
                if (Cells[i,j]) count++;

        // ���̃Z���������Ă���ꍇ
        if (Cells[x,z])
        {
            // ����:���̃Z���������Ă��� + �אڂ��鐶�����Z���� 2 or 3
            // �ߑa��:���̃Z���������Ă��� + �אڂ��鐶�����Z���� 1 �ȉ�
            // �ߖ���:���̃Z���������Ă��� + �אڂ��鐶�����Z���� 4 �ȏ�
            return 3 <= count && count <= 4;
        }
        // ���̃Z��������ł���ꍇ
        else
        {
            // �a��:���̃Z��������ł��� + �אڂ��鐶�����Z���� 3
            return count == 3;
        }
    }

    /// <summary>���̐���̃Z����Ԃ�</summary>
    public bool[,] GetNextCells()
    {
        bool[,] next = new bool[_rowLength, _colLength];
        for (int i = 0; i < _rowLength; i++)
            for (int j = 0; j < _colLength; j++)
                next[i, j] = CheckCell(i, j);
        return next;
    }

    /// <summary>���݂̃Z����\������</summary>
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

    /// <summary>���̂�ǉ�����</summary>
    public void AddObject()
    {
        // �����ɔC�ӂ̃Z����ǉ����鏈��������
        GenerateGlider();
    }

    // 4�p���烉���_���ŃO���C�_�[����������
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
