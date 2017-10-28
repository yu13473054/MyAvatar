using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {
    public const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";
    public static string[] _heads = { "ch_pc_hou_004_tou", "ch_pc_hou_006_tou", "ch_pc_hou_008_tou" };
    public static string[] _bodys = { "ch_pc_hou_004_shen", "ch_pc_hou_006_shen", "ch_pc_hou_008_shen" };
    public static string[] _hands = { "ch_pc_hou_004_shou", "ch_pc_hou_006_shou", "ch_pc_hou_008_shou" };
    public static string[] _foots = { "ch_pc_hou_004_jiao", "ch_pc_hou_006_jiao", "ch_pc_hou_008_jiao" };
    public static string[] _weapons = { "ch_we_one_hou_004", "ch_we_one_hou_006", "ch_we_one_hou_008" };

    AvatarCtrl _avatarCtrl;
    private int _headIndex, _bodyIndex, _handIndex, _footIndex, _weaponIndex;
    private bool _combine;
    private int _animState;

	void Start () {
        GameObject skeletonGo = Instantiate(Resources.Load("ch_pc_hou"), transform) as GameObject;

	    _avatarCtrl = new AvatarCtrl(skeletonGo);
	    _combine = true;
        _avatarCtrl.Equip(_headIndex,_bodyIndex, _handIndex, _footIndex, _combine);
        _avatarCtrl.EquipWeapon(_weaponIndex);
        _avatarCtrl.PlayAnim(_animState);

	}



    void Update () {
        if (_animState == 1)
        {
            _avatarCtrl.Update();
        }
	}

    void OnGUI()
    {
        for (int i = 0; i < _weapons.Length; i++)
        {
            if (GUI.Button(new Rect(i * 100, 30, 100, 50), "Weapon" + (i == _weaponIndex ? "(√)" : "")))
            {
                if (i != _weaponIndex)
                {
                    _weaponIndex = i;
                    _avatarCtrl.EquipWeapon(_weaponIndex);
                }
            }
        }
        bool isChange = false;
        for (int i = 0; i < _heads.Length; i++)
        {
            if (GUI.Button(new Rect(i * 100, 80, 100, 50), "Head" + (i == _headIndex ? "(√)" : "")))
            {
                if (i != _headIndex)
                {
                    _headIndex = i;
                    isChange = true;
                }
            }
        }

        for (int i = 0; i < _bodys.Length; i++)
        {
            if (GUI.Button(new Rect(i * 100, 130, 100, 50), "Chest" + (i == _bodyIndex ? "(√)" : "")))
            {
                if (i != _bodyIndex)
                {
                    _bodyIndex = i;
                    isChange = true;
                }
            }
        }

        for (int i = 0; i < _hands.Length; i++)
        {
            if (GUI.Button(new Rect(i * 100, 180, 100, 50), "Hand" + (i == _handIndex ? "(√)" : "")))
            {
                if (i != _handIndex)
                {
                    _handIndex = i;
                    isChange = true;
                }
            }
        }

        for (int i = 0; i < _foots.Length; i++)
        {

            if (GUI.Button(new Rect(i * 100, 230, 100, 50), "Feet" + (i == _footIndex ? "(√)" : "")))
            {

                if (i != _footIndex)
                {
                    _footIndex = i;
                    isChange = true;
                }
            }
        }

        if (GUI.Button(new Rect(Screen.width - 150, 100, 150, 50), _combine ? "Merge materials(√)" : "Merge materials"))
        {
            _combine = !_combine;
        }

        if (isChange) _avatarCtrl.Equip(_headIndex, _bodyIndex, _handIndex, _footIndex, _combine);


        if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 50), _animState == 0 ? "Attack" : "Stand"))
        {
            if (_animState == 0) _animState = 1;
            else if (_animState == 1) _animState = 0;
            _avatarCtrl.PlayAnim(_animState);
        }

//        if (GUI.Button(new Rect(Screen.width - 100, 50, 100, 50), character.rotate ? "Static" : "Rotate"))
//        {
//            if (character.rotate)
//            {
//                character.rotate = false;
//            }
//            else
//            {
//                character.rotate = true;
//            }
//        }
    }
}
