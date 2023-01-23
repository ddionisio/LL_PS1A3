﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace M8.Animator {
	[RequireComponent(typeof(Renderer))]
	public class AMMaterialController : MonoBehaviour {
	    
	    private Renderer mRenderer;
	    private Material[] mMaterialsDefault;
	    private Material[] mMaterialsCurrent;

	    private Dictionary<Material, Material>[] mMaterialInstances;

	    public void Revert() {
	        mRenderer.sharedMaterials = mMaterialsDefault;    
	    }

	    public void Revert(int matInd) {
	        mMaterialsCurrent[matInd] = mMaterialsDefault[matInd];
	        mRenderer.sharedMaterials = mMaterialsCurrent;
	    }

	    public Material Instance(int matInd, Material mat) {
	        Material matInst;
	        if(!mMaterialInstances[matInd].TryGetValue(mat, out matInst)) {
	            mMaterialInstances[matInd].Add(mat, matInst = new Material(mat));
	        }
	        return matInst;
	    }

	    /// <summary>
	    /// Apply the given material, returns the instance of the material
	    /// </summary>
	    public Material Apply(int matInd, Material mat) {
	        Material matInst = Instance(matInd, mat);
	        if(mMaterialsCurrent[matInd] != matInst) {
	            mMaterialsCurrent[matInd] = matInst;
	            mRenderer.sharedMaterials = mMaterialsCurrent;
	        }

	        return matInst;
	    }

	    void OnDestroy() {
	        for(int i = 0; i < mMaterialsCurrent.Length; i++) {
	            foreach(var pair in mMaterialInstances[i])
	                Destroy(pair.Value);
	            mMaterialInstances[i].Clear();
	        }
	    }

	    void Awake() {
	        mRenderer = GetComponent<Renderer>();

	        mMaterialsDefault = mRenderer.sharedMaterials;

	        //initialize
	        int count = mMaterialsDefault.Length;
	        
	        mMaterialsCurrent = new Material[count];
	        mMaterialInstances = new Dictionary<Material, Material>[count];

	        for(int i = 0; i < count; i++) {
	            mMaterialsCurrent[i] = mMaterialsDefault[i];
	            mMaterialInstances[i] = new Dictionary<Material, Material>();
	        }
	    }
	}
}