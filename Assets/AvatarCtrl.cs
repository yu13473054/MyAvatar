using System.Collections.Generic;
using UnityEngine;

/*
 Avatar换装流程：
 * 1，每一套装备模型必须使用同一套骨骼，并单独将骨骼数据保存成一个Prefab。
 *    骨骼数据在Unity中的展示形式就是Transform。游戏开始时，首先将骨骼实例化。
 * 2，将模型拆分成多个部分，将每一个部分单独保存成Prefab，每一个Prefab都含有自身的SkinnedMeshRenderer。
 * 3，实例化各个部位，同时遍历每个部位的SkinnedMeshRenderer，用于后边的SkinnedMeshRenderer合并。
 * 4，同时遍历每个部位的SkinnedMeshRenderer上绑定的骨骼数据，然后保存。用于设置最后生成的SkinnedMeshRenderer的骨骼数据
 * 5，将新生成的SkinnedMeshRenderer加到骨骼的实例上，就可以实现avatar换装了
 * 注：各个部位中保存的骨骼数据必须和骨骼模型的名称一致，从而达到共用一套骨骼
 */

/*
CombineMesh流程：
*  1，收集所有需要combine的SkinnedMeshRenderer
2，遍历每个SkinnedMeshRenderer，获取到每个SkinnedMeshRenderer中的material、CombineInstance,
（SkinnedMeshRenderer.materials.Length 和 SkinnedMeshRenderer.sharedMesh.subMeshCount 应该是一样的，并且一一对应。待测试
    也就是最终得到的material数组长度和CombineInstance数组长度一致，并且一一对应）
此时获得materials的个数和CombineInstance的个数应该是一样的
* 3，获取每个material中Texture2D，将这些纹理打包到一张贴图上，此时会返回一个Rect[]，这个里边保存着每一个纹理的在大图中的UV信息，
    并且顺序和传入的Texture2D一致
* 4，遍历CombineInstance的list，修改存储的每个Instance中的mesh的UV信息。因为此时小图都合并到大图上了，需要重新设置
5，建立一个新的SkinnedMeshRenderer，重新设置其材质，mesh信息
*/

public class AvatarCtrl
{
    private GameObject _skel;
    private Animation _anim;
    private Transform _weaponRoot;

    public AvatarCtrl(GameObject skel)
    {
        this._skel = skel;
        _anim = skel.GetComponent<Animation>();

    }

    public void Equip(int headIndex, int bodyIndex, int handIndex, int footIndex, bool combine)
    {
        string[] equipNames = { Main._heads[headIndex], Main._bodys[bodyIndex], Main._hands[handIndex], Main._foots[footIndex] };
        GameObject[] tempGO = new GameObject[equipNames.Length];
        SkinnedMeshRenderer[] meshes = new SkinnedMeshRenderer[equipNames.Length];
        for (int i = 0; i < equipNames.Length; i++)
        {
            GameObject go = GameObject.Instantiate(Resources.Load<GameObject>(equipNames[i]));
            meshes[i] = go.GetComponentInChildren<SkinnedMeshRenderer>();
            tempGO[i] = go;
        }
        CombineMesh(_skel, meshes, true);

        for (int i = 0; i < tempGO.Length; i++)
        {
            GameObject.DestroyImmediate(tempGO[i]);
        }
    }

    void CombineMesh(GameObject skel, SkinnedMeshRenderer[] meshes, bool combine)
    {
        List<Transform> transforms = new List<Transform>(skel.GetComponentsInChildren<Transform>(true));

        List<Material> materials = new List<Material>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Transform> bones = new List<Transform>();

        for (int i = 0; i < meshes.Length; i++)
        {
            // Collect materials
            SkinnedMeshRenderer smr = meshes[i];
            materials.AddRange(smr.materials);

            // Collect meshes
            for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
            {
                CombineInstance co = new CombineInstance()
                {
                    mesh = smr.sharedMesh,
                    subMeshIndex = j
                };
                combineInstances.Add(co);
            }
            // Collect bones
            for (int j = 0; j < smr.bones.Length; j++)
            {
                string smrBoneName = smr.bones[j].name;
                for (int k = 0; k < transforms.Count; k++)
                {
                    if (smrBoneName.Equals(transforms[k].name))
                    {
                        bones.Add(transforms[k]);
                        break;
                    }
                }
            }
        }

        List<Vector2[]> oldUVs = null;
        Material newM = null;
        if (combine)
        {
            newM = new Material(Shader.Find("Mobile/Diffuse"));
            oldUVs = new List<Vector2[]>();

            List<Texture2D> texture2Ds = new List<Texture2D>();
            for (int i = 0; i < materials.Count; i++)
            {
                texture2Ds.Add(materials[i].GetTexture(Main.COMBINE_DIFFUSE_TEXTURE) as Texture2D);
            }
            Texture2D newDiffuseTex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            Rect[] uvs = newDiffuseTex.PackTextures(texture2Ds.ToArray(), 0);
            newM.mainTexture = newDiffuseTex;

            Vector2[] uva, uvb;
            for (int i = 0; i < combineInstances.Count; i++)
            {
                uva = combineInstances[i].mesh.uv;
                uvb = new Vector2[uva.Length];
                for (int j = 0; j < uva.Length; j++)
                {
//                    uvb[j] = new Vector2(uva[j].x * uvs[i].width + uvs[i].x, uva[j].y * uvs[i].height + uvs[i].y);
                    uvb[j].x = Mathf.Lerp(uvs[i].xMin, uvs[i].xMax, uva[j].x);
                    uvb[j].y = Mathf.Lerp(uvs[i].yMin, uvs[i].yMax, uva[j].y);
                }
                oldUVs.Add(uva);
                combineInstances[i].mesh.uv = uvb;
            }
        }

        SkinnedMeshRenderer oldSkinned = skel.GetComponent<SkinnedMeshRenderer>();
        if (oldSkinned != null) GameObject.DestroyImmediate(oldSkinned);

        SkinnedMeshRenderer newSMR = skel.AddComponent<SkinnedMeshRenderer>();
        newSMR.sharedMesh = new Mesh();
        newSMR.sharedMesh.CombineMeshes(combineInstances.ToArray(), combine, false);
        newSMR.bones = bones.ToArray();

        if (combine)
        {
            //还原UV信息，防止修改了FBX的UV
            for (int i = 0; i < combineInstances.Count; i++)
            {
                combineInstances[i].mesh.uv = oldUVs[i];
            }
            newSMR.material = newM;
        }
        else
        {
            newSMR.materials = materials.ToArray();
        }
    }

    public void EquipWeapon(int weaponIndex)
    {
        if (_weaponRoot == null)
        {
            Transform[] transforms = _skel.GetComponentsInChildren<Transform>();
            foreach (Transform joint in transforms)
            {
                if (joint.name == "weapon_hand_r")
                {// find the joint (need the support of art designer)
                    _weaponRoot = joint.gameObject.transform;
                    break;
                }
            }
        }
        if (_weaponRoot.transform.childCount > 0)
        {
            GameObject.Destroy(_weaponRoot.transform.GetChild(0).gameObject);
        }
        Object res = Resources.Load(Main._weapons[weaponIndex]);
        GameObject weaponGo = GameObject.Instantiate(res) as GameObject;
        weaponGo.transform.parent = _weaponRoot;
        weaponGo.transform.localPosition = Vector3.zero;
        weaponGo.transform.localScale = Vector3.one;
        weaponGo.transform.localRotation = Quaternion.identity;

    }

    public void PlayAnim(int animState)
    {
        if (animState == 0)
        {
            _anim.wrapMode = WrapMode.Loop;
            _anim.Play("breath");
        }
        else if (animState == 1)
        {
            _anim.wrapMode = WrapMode.Once;
            _anim.PlayQueued("attack1");
            _anim.PlayQueued("attack2");
            _anim.PlayQueued("attack3");
            _anim.PlayQueued("attack4");
        }
    }

    public void Update()
    {
        if (!_anim.isPlaying)
        {
            PlayAnim(1);
        }
    }
}
