using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace M8.Animator {
	[AddComponentMenu("")]
	public class AMEventTrack : AMTrack {

		[SerializeField]
	    GameObject obj;

	    public override bool canTween { get { return false; } }

		protected override void SetSerializeObject(UnityEngine.Object obj) {
			this.obj = obj as GameObject;
		}
		
		protected override UnityEngine.Object GetSerializeObject(GameObject targetGO) {
			return targetGO ? targetGO : obj;
		}

	    public override string getTrackType() {
	        return "Event";
	    }

	    // add a new key
	    public void addKey(AMITarget itarget, OnAddKey addCall, int _frame) {
	        foreach(AMEventKey key in keys) {
	            // if key exists on frame, do nothing
	            if(key.frame == _frame) {
	                return;
	            }
	        }
	        AMEventKey a = addCall(gameObject, typeof(AMEventKey)) as AMEventKey;
	        a.frame = _frame;
	        // add a new key
	        keys.Add(a);
	        // update cache
			updateCache(itarget);
	    }
		public bool hasSameEventsAs(AMITarget target, AMEventTrack _track) {
			if(_track.GetTarget(target) == GetTarget(target))
	            return true;
	        return false;
	    }

		public override AnimatorTimeline.JSONInit getJSONInit(AMITarget target) {
	        // no initial values to set
	        return null;
	    }

	    public override List<GameObject> getDependencies(AMITarget target) {
			GameObject go = GetTarget(target) as GameObject;
	        List<GameObject> ls = new List<GameObject>();
			if(go) ls.Add(go);
	        foreach(AMEventKey key in keys) {
	            ls = ls.Union(key.getDependencies()).ToList();
	        }
	        return ls;
	    }

		public override List<GameObject> updateDependencies(AMITarget target, List<GameObject> newReferences, List<GameObject> oldReferences) {
			GameObject go = GetTarget(target) as GameObject;
	        bool didUpdateObj = false;
	        bool didUpdateParameter = false;
			if(go) {
	            for(int i = 0; i < oldReferences.Count; i++) {
					if(oldReferences[i] == go) {
	                    // check if new GameObject has all the required components
	                    foreach(AMEventKey key in keys) {
	                        string componentName = key.getComponentName();
	                        if(newReferences[i].GetComponent(componentName) == null) {
	                            // missing component
	                            Debug.LogWarning("Animator: Event Track component '" + componentName + "' not found on new reference for GameObject '" + obj.name + "'. Duplicate not replaced.");
	                            List<GameObject> lsFlagToKeep = new List<GameObject>();
	                            lsFlagToKeep.Add(oldReferences[i]);
	                            return lsFlagToKeep;
	                        }
	                    }
						SetTarget(target, newReferences[i].transform);
	                    didUpdateObj = true;
	                    break;
	                }

	            }
	        }
	        foreach(AMEventKey key in keys) {
				if(key.updateDependencies(newReferences, oldReferences, didUpdateObj, go) && !didUpdateParameter) didUpdateParameter = true;
	        }

	        if(didUpdateObj || didUpdateParameter) updateCache(target);

	        return new List<GameObject>();
	    }

	    protected override void DoCopy(AMTrack track) {
	        (track as AMEventTrack).obj = obj;
	    }
	}
}