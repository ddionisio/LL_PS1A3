using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockConnectController : M8.SingletonBehaviour<BlockConnectController> {
    private Dictionary<string, int> mConnects = new Dictionary<string, int>();

    public event System.Action<string, bool> connectUpdateCallback;

    public bool IsConnected(string id) {
        int counter = 0;
        mConnects.TryGetValue(id, out counter);

        return counter > 0;
    }

    public void SetConnect(string id, bool connect) {
        if(connect) {
            bool isUpdate = false;

            if(mConnects.ContainsKey(id)) {
                int val = mConnects[id];
                val++;
                mConnects[id] = val;

                isUpdate = val == 1;
            }
            else {
                mConnects.Add(id, 1);
                isUpdate = true;
            }

            if(isUpdate && connectUpdateCallback != null)
                connectUpdateCallback(id, true);
        }
        else {
            if(mConnects.ContainsKey(id)) {
                int val = mConnects[id];
                if(val > 0) val--;
                mConnects[id] = val;

                if(val == 0 && connectUpdateCallback != null)
                    connectUpdateCallback(id, false);
            }
        }
    }
}
