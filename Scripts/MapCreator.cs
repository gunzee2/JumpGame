using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// マップ自動生成クラス.
/// </summary>
public class MapCreator : MonoBehaviour {

	// 定数.
	public static float SCREEN_WIDTH = 18f;	                // 横に置けるブロックの最大数.
	public static float SCREEN_HEIGHT = 31f;            	// 縦に置けるブロックの最大数.

	public static float BLOCK_WIDTH = 2.0f;		        	// ブロックの幅.
	public static float BLOCK_HEIGHT = 1.0f;	        	// ブロックの高さ.
	
	public static float PATTERN_VERTICAL_DISTANCE = 5f;	    // パターンとパターンの縦幅の差(固定値).

    // ブロック情報の構造体.
	public struct FloorBlock {
		public bool is_created;					        	// ブロックが作成済みか否か.
		public Vector2 position;				        	// ブロックの位置.
	}

    // メンバ変数.
	public List<GameObject> blockPatterns;		        	// ブロックパターンのリスト.
	public int currentAltitude = 1;	                    	// 現在標高.

	private GameObject player = null;				    	// シーン上のPlayerを保管.
	private FloorBlock last_block;                          // 直近に生成したブロックの情報.

	// Use this for initialization
	void Start () {

		// プレイヤーオブジェクトを取得.
		this.player = GameObject.FindGameObjectWithTag("Player");

		// 直近のブロックは未作成とする.
		this.last_block.is_created = false;
	
	}
	
	// Update is called once per frame
	void Update () {
		// プレイヤーのY位置を取得.
		float block_generate_y = this.player.transform.position.y;

        // プレイヤーの位置より画面1.25個分上の位置をブロック生成しきい値とする.
		block_generate_y += SCREEN_HEIGHT * 1.25f;

		// 最後に作ったブロックの位置がしきい値より小さい間.
		while(this.last_block.position.y < block_generate_y) {
			// ブロックを作る.
			create_floor_block();
			currentAltitude += 1;	// 標高を1増加する.
		}
	
	}

    /// <summary>
    /// 床ブロックの生成.
    /// </summary>
	void create_floor_block() {
		
		Vector2 block_position;
		
		// 前のブロックが未作成の時.
		if(!this.last_block.is_created) {
			// ブロックの位置を、とりあえず真ん中にする.
			block_position = new Vector2(8, 0);
			// それから、ブロックのY位置を半画面分、下に移動.
			block_position.y -= (float)SCREEN_HEIGHT / 2.0f;
            // 次に作るブロックの位置を、足場一つ分上に移動(更にスタート地点の地面分+0.5f上にする).
            block_position.y += PATTERN_VERTICAL_DISTANCE + 0.5f;
		} else { // last_blockが作成済みの場合.
			// 今回作るブロックの位置を、前回作ったブロックと同じに.
			block_position = this.last_block.position;
		}


		// 作成するブロックのパターン番号をランダムに決定.
		int block_template_num = Random.Range(0,this.blockPatterns.Count);
		
		// パターンからブロック群を作成する.
		create_blocks_from_pattern(block_position, block_template_num);
	
		// 次に作るブロック群の位置を、上に移動(1パターン2段のため、二段階上に上げる).
		block_position.y += PATTERN_VERTICAL_DISTANCE * 2;

		// last_blockの位置を、今回の位置に更新.
		this.last_block.position = block_position;
		// ブロック作成済みなので、last_blockのis_createdをtrueに.
		this.last_block.is_created = true;

	}

    /// <summary>
    /// パターンプレハブからブロックを生成する.
    /// </summary>
    /// <param name="block_pattern_pos"></param>
    /// <param name="template_num"></param>
	void create_blocks_from_pattern(Vector2 block_pattern_pos, int template_num) {
		// ブロックパターンを生成し、goに保管.
		GameObject go = GameObject.Instantiate(this.blockPatterns[template_num]) as GameObject;
		go.transform.position = block_pattern_pos;	// ブロックパターンの位置を移動.

        // 子ブロックに標高をセット.
        setBlockAltitude(go);
	
	}

    /// <summary>
    /// ブロックパターンに含まれる子ブロックに標高をセットする.
    /// </summary>
    /// <param name="blockPatternObj">ブロックパターンオブジェクト</param>
    private void setBlockAltitude(GameObject blockPatternObj)
    {
		// 子のBlockオブジェクトからBlockControllerを取り出し、得点計算に使用する「標高」をセット.
		// 標高はパターン１つにつき同一とする(二段のパターンでも、全てのブロックが全て同じ標高になる).
        BlockController[] blocks = blockPatternObj.GetComponentsInChildren<BlockController>();
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].altitude = this.currentAltitude;
        }
    }



}
