using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// タイトル画面表示クラス.
/// </summary>
public class TitleScript : MonoBehaviour {

    [SerializeField]
	private Text flashing_text;                 // 点滅表示させるテキストオブジェクト.
    [SerializeField]
	private AudioClip startSound;               // ゲームをスタートした時の音.

	private ScoreCounter score_counter = null;  // スコアカウンタへの参照(ハイスコアの表示に使用).
	private float interval_time = 0.0f;         // 0.5秒ごとにテキストを点滅させる用タイマー.

	// Use this for initialization
	void Start () {
		this.score_counter = this.gameObject.GetComponent<ScoreCounter>();
	}

	
	// Update is called once per frame
	void Update () {

        // GAME START表示を点滅させる.
		interval_time += Time.deltaTime;
		if(interval_time > 0.5f) {
			if(flashing_text.enabled) {
				this.flashing_text.enabled = false;
			} else {
				this.flashing_text.enabled = true;
			}
			interval_time = 0;
		}
		
        // マウスボタンが押されたらゲームスタート.
		if(Input.GetMouseButtonDown(0)) {
            // テキスト点滅をやめさせて表示状態にする.
			this.flashing_text.enabled = true;
			interval_time = 0;
            // ゲームスタート処理実行.
			StartCoroutine("GameStart");
		}
        // ESCボタンを押すとゲーム終了.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

	}

    /// <summary>
    /// ゲーム開始処理.
    /// </summary>
    /// <returns></returns>
	private IEnumerator GameStart() {

        // ゲームスタート音を再生.
		this.GetComponent<AudioSource>().PlayOneShot(startSound);	
		// 0.4秒待つ
		yield return new WaitForSeconds (0.4f);
        // ゲームシーンをロード.
		Application.LoadLevel("GameScene");	

	
	}
}

