using UnityEngine;
using System.Collections;

/// <summary>
/// ゲームの全体制御クラス.
/// </summary>
public class GameRoot : MonoBehaviour
{

    [SerializeField]
    private AudioClip fallSound;                    // 落下音.

	private PlayerController player = null;         // プレイヤーコントローラーへの参照(落下判定に使用).
	private ScoreCounter score_counter = null;      // スコアカウンタへの参照(ハイスコアのセーブに使用).
	private bool isGameOver = false;	            // ゲームオーバーフラグ.

    // Use this for initialization
	void Start () {
	
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		this.score_counter = this.gameObject.GetComponent<ScoreCounter>();

		// ターゲットフレームレートを60fpsに.
		Application.targetFrameRate = 60;
	}
	
	// Update is called once per frame
	void Update () {

        // ESCボタンを押すとゲーム終了.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

		// プレイヤーが落下した時、ゲームオーバーとする.
		if(player.isFalled && !this.isGameOver) {
			// ゲームオーバーコルーチンの実行.
			StartCoroutine("GameOver");
			// コルーチン実行中に再度この処理が流れないよう、ゲームオーバーフラグをONにする.
			this.isGameOver = true;
			
		}
	
	}

    /// <summary>
    /// ゲームオーバー処理.
    /// </summary>
    /// <returns></returns>
	private IEnumerator GameOver() {

		// ハイスコアのセーブ.
		this.score_counter.Save();

        // BGM停止.
		this.GetComponent<AudioSource>().Stop(); 
		this.GetComponent<AudioSource>().loop = false;
		// 落下音再生.
		this.GetComponent<AudioSource>().PlayOneShot(fallSound);

		// 1秒待つ
		yield return new WaitForSeconds (1.0f);

		// タイトル画面へ戻る.
		Application.LoadLevel("TitleScene");
	
	}
}
