using UnityEngine;
using System.Collections;

/// <summary>
/// カメラの移動制御クラス.
/// </summary>
public class CameraController : MonoBehaviour {

    private PlayerController playerController = null;   // PlayerControllerへの参照(座標と着地判定を使用する).
    private Vector3 position_offset = Vector3.zero;     // カメラの中心とプレイヤー位置のオフセット.
    private Vector3 new_position;                       // 更新後(プレイヤー追従＆オフセット計算後)のカメラ座標.

    public float damping;                               // 追従移動の滑らかさ.

	// Use this for initialization
	void Start () {

		// Playerオブジェクトを探し、PlayerControllerコンポーネントを取得.
		this.playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();;

		// カメラとプレイヤー間の差を取得し、更にYを5.0f分下にずらすことで、プレイヤーの下に余白をつくる.
		// ただしこの差分が適用されるのは1標高上がったところから(初期位置では適用されない).
		this.position_offset = this.transform.position - this.playerController.transform.position;
		this.position_offset.y = this.position_offset.y - 5.5f;

        // 更新後のカメラ座標をとりあえず現在位置と同じにする.
		new_position = this.transform.position;
	}
	
	// LateUpdate()は全てのGameObjectのUpdate処理が終わった後に実行される.
	void Update () {

		// Playerが着地中かつ、プレイヤーのY座標がカメラのY座標(差分含む)よりも高い時カメラ座標を更新する.
		if(this.playerController.isGrounded && this.playerController.transform.position.y + this.position_offset.y > this.transform.position.y ) {
			// カメラの現在位置をnew_positionに取得.
			new_position = this.transform.position;
			// プレイヤーのY座標に差分を足して、変数new_positionのYに代入する.
			new_position.y = this.playerController.transform.position.y + this.position_offset.y;
			
		}

		// カメラの位置を、新しい位置に更新.
		this.transform.position = Vector3.Lerp(this.transform.position, new_position, damping * Time.deltaTime);
	}

}
