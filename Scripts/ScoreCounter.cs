using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// スコア計算、更新、表示用クラス.
/// </summary>
public class ScoreCounter : MonoBehaviour {

    public int score;                   // 最後のスコア.
    public int highScore;               // ハイスコア.

    public Text score_text;             // スコア表示用Textオブジェクト.
    public Text highscore_text;         // ハイスコア表示用Textオブジェクト.

    private string highScoreKey = "highScore";  // PlayerPrefsでハイスコアを保存するためのキー.

    // Use this for initialization
    void Start () {
        initializeScore();
    }
    
    // Update is called once per frame
    void Update () {
        updateScoreGUI();
    }

    /// <summary>
    /// スコア加算.
    /// </summary>
    /// <param name="addPoint">加算するポイント.</param>
    public void AddScore(int addPoint) {

        this.score = this.score + addPoint;

        // 現在スコアがハイスコアを上回ったら、現在スコアをハイスコアにする.
        if(this.score > this.highScore) {
            this.highScore = this.score;
        }
    }

    /// <summary>
    /// ハイスコア保存.
    /// </summary>
    public void Save() {
        Debug.Log("highScore Save:" + this.highScore);
        PlayerPrefs.SetInt(this.highScoreKey, this.highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 初期化処理.
    /// </summary>
    private void initializeScore () {
        
        this.score = 0;
        // ハイスコア読み込み(保存されていなければ0を読む).
        this.highScore = PlayerPrefs.GetInt(this.highScoreKey, 0);
        Debug.Log("highScore Load:" + this.highScore);
    }

    /// <summary>
    /// スコアGUI更新.
    /// </summary>
    private void updateScoreGUI()
    {
        this.printValue(this.score_text, "SCORE:", this.score);
        this.printValue(this.highscore_text, "BEST:", this.highScore);
    }

    /// <summary>
    /// テキストオブジェクトの表示更新.
    /// </summary>
    /// <param name="txtObj">更新したいテキストオブジェクト.</param>
    /// <param name="label">ラベル(数値の前に表示される文字).</param>
    /// <param name="value">表示したい数値.</param>
    private void printValue(Text txtObj, string label, int value) {

        txtObj.text = label + value.ToString();
    }
}
