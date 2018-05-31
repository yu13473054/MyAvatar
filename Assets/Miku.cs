using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Miku : MonoBehaviour {

	void Start () {
//        GameObject skeletonGo = Instantiate(Resources.Load<GameObject>("Avatar/mikuAvatar/fbx/miku_chest_lqsn_001_035"), transform);

        Stopwatch sw = Stopwatch.StartNew();

        GameObject skeletonGo = Instantiate(Resources.Load<GameObject> ("Avatar/commonAvatar/miku_skeleton"), transform);

        GameObject hairGo = Instantiate(Resources.Load<GameObject> ("Avatar/mikuAvatar/fbx/miku_hair_029_000"), transform);
        List<SkinnedMeshRenderer> smrList = new List<SkinnedMeshRenderer>();

	    SkinnedMeshRenderer[] smr = hairGo.GetComponentsInChildren<SkinnedMeshRenderer>();
	    for (int i = 0; i < smr.Length; i++)
	    {
	        SkinnedMeshRenderer tmpSMR = smr[i];
	        if (tmpSMR.name.Contains("_hair"))
	        {
	            tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_hair_029_000");
	        }
            smrList.Add(tmpSMR);
	    }

	    GameObject headGo = Instantiate(Resources.Load<GameObject> ("Avatar/mikuAvatar/fbx/miku_head_001_000"), transform);
        smr = headGo.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < smr.Length; i++)
        {
            SkinnedMeshRenderer tmpSMR = smr[i];
            if (tmpSMR.name.Contains("cheek"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_cheek_001_000");
            }
            else if (tmpSMR.name.Contains("face"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_head_001_000");
            }
            smrList.Add(tmpSMR);
        }

        GameObject handGo = Instantiate(Resources.Load<GameObject> ("Avatar/mikuAvatar/fbx/miku_hand_tnl_001_003"), transform);
        smr = handGo.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < smr.Length; i++)
        {
            SkinnedMeshRenderer tmpSMR = smr[i];
            if (tmpSMR.name.Contains("_hand"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_hand_tnl_001_000");
            }
            smrList.Add(tmpSMR);
        }

        GameObject suitGo = Instantiate(Resources.Load<GameObject> ("Avatar/mikuAvatar/fbx/miku_suit_tnl_001_035"), transform);
        smr = suitGo.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < smr.Length; i++)
        {
            SkinnedMeshRenderer tmpSMR = smr[i];
            if (tmpSMR.name.Contains("_body"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_body_001_000");
            }
            else if (tmpSMR.name.Contains("_leg"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_body_001_000");
            }
            else if (tmpSMR.name.Contains("_shoe"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_shoe_tnl_001_000");
            }
            else if (tmpSMR.name.Contains("_sock"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_sock_tnl_001_000");
            }
            else if (tmpSMR.name.Contains("_suit"))
            {
                tmpSMR.material = Resources.Load<Material>("Avatar/mikuAvatar/material/miku_suit_tnl_001_000");
            }
            smrList.Add(tmpSMR);
        }

        CombineMesh(skeletonGo,smrList.ToArray(),false);

        Destroy(hairGo);
        Destroy(headGo);
        Destroy(handGo);
        Destroy(suitGo);

	    Animator animCtrl = skeletonGo.GetComponent<Animator>();
	    animCtrl.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Art\\Controller\\Miku\\miku_controller_battle");

        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
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
            newM = new Material(Shader.Find("Miyoo/Character"));
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


    void Update () {
		
	}
}
