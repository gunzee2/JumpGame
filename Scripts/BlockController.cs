using UnityEngine;
using System.Collections;

public class BlockController : MonoBehaviour {

    public int altitude = 0;    // 標高.

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    void OnBecameInvisible() {
        GameObject.Destroy(this.gameObject);
    }
}
