using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    [SerializeField] private int _houseCode;

    private SpriteRenderer spriteRenderer;

    public int HouseCode { get { return _houseCode; } set { _houseCode = value; } }

    [SerializeField] private bool _isHouseRepaired01 = false;
    public bool IsHouseRepaired01 { get => _isHouseRepaired01; set => _isHouseRepaired01 = value; }
    [SerializeField] private bool _isHouseRepaired02 = false;
    public bool IsHouseRepaired02 { get => _isHouseRepaired02; set => _isHouseRepaired02 = value; }
}
