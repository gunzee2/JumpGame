using UnityEngine;
using System.Collections;

/// <summary>
/// 足場ブロックのパターン制御クラス.
/// </summary>
public class BlockPatternController : MonoBehaviour {

    // メンバ変数.
	public MapCreator map_creator = null;	// MapCreatorを保管するための変数.

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		// 見切れている時.
		if(isOutofScreen(this.gameObject)) {
			// 自分自身を削除.
			GameObject.Destroy(this.gameObject);
		}
	}

    /// <summary>
    /// ブロックパターンが見切れているか判別する関数.
    /// </summary>
    /// <param name="block_pattern">ブロックパターン</param>
    /// <returns>見切れているかどうか</returns>
	private bool isOutofScreen(GameObject block_pattern) {
		bool ret = false;	// 戻り値.

		// カメラの中心から、半画面-7f分下の位置。これが、消えるべきか否かを決める閾値となる.
		float down_limit = Camera.main.transform.position.y - (MapCreator.SCREEN_HEIGHT / 2.0f) - 7f;

		// ブロックパターンの位置がしきい値より小さい(下)なら.
		if(block_pattern.transform.position.y < down_limit) {
			ret = true;		// 戻り値をtrueに.
		}

		return ret;
	}
}
