using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーの操作用クラス.
/// </summary>
public class PlayerController : MonoBehaviour
{

	// 定数.
    public static float JUMP_GRACE_TIME = 0.25f;			// ジャンプしたい意思を受け付ける時間.
    public static float JUMP_HEIGHT_MAX = 20.0f;				// ジャンプの高さ.
    public static float JUMP_KEY_RELEASE_REDUCE = 0.4f;		// ジャンプからの減速率.

    // Playerの各種状態を表すデータ型.
    public enum STEP
    {
        NONE = -1,	// 状態情報なし.
        RUN = 0,	// 走る.
        JUMP,		// ジャンプ.
        NUM,		// 状態が何種類あるかを示す(=2).
    };

	// メンバ変数.
    public bool isGrounded = true;      // 着地中にtrueになり、ジャンプするとfalseになる.
    public bool isFalled = false;       // 画面外に落下したかどうか.

    [SerializeField] private int coinPoint;             // コインの加算ポイント.
    [SerializeField] private int climbPoint;            // ブロックを登った際の加算ポイント.
    [SerializeField] private float up_power;            // ジャンプ力.
    [SerializeField] private float move_speed;          // 移動速度.
    [SerializeField] private LayerMask groundLayer;     // 地面のレイヤーマスク(衝突判定に使用).
    [SerializeField] private AudioClip jumpSound;       // ジャンプした時の音.
    [SerializeField] private AudioClip hitWallSound;    // 天井、壁(側面)にあたった時の音.
    [SerializeField] private AudioClip coinSound;       // コインをゲットした時の音.

    private GameObject root;                // GameRootへの参照(スコア計算時に使用).
    private Rigidbody2D rigidbody2D;
    private BoxCollider2D collider;
    private Animator animator;
    private AudioSource audio;

    private STEP step = STEP.NONE;              // Playerの現在の状態.
    private STEP next_step = STEP.NONE;	        // Playerの次の状態.

    private float highest_position_y;           // ジャンプ前の標高.
    private float step_timer = -1.0f;           // 状態変化までの経過時間.
    private float jump_timer = -1.0f;           // ボタンダウン後の経過時間(ジャンプ強弱をつけるため).

    private bool facingRight = false;           // 右を向くとtrue、左を向くとfalse.
    private bool is_velocity_reduced = false;   // 減速済みかどうか.
    private bool is_button_pressing = false;    // ボタンを押している間かどうか.


    void Start()
    {
        this.root = GameObject.FindGameObjectWithTag("GameController");
        this.rigidbody2D = this.GetComponent<Rigidbody2D>();
        this.audio = this.GetComponent<AudioSource>();
        this.collider = this.GetComponent<BoxCollider2D>();
        this.animator = this.GetComponent<Animator>();
        this.next_step = STEP.RUN;
        this.rigidbody2D.velocity = new Vector2(-this.move_speed, 0);
        this.highest_position_y = this.transform.position.y;
    }

    // 全体の流れ.
    // 入力受付 => 次の状態 の更新 => 現在の状態 の更新 => 速度更新 => Animatorに反映.
    void Update()
    {
        updateTimer();                      // タイマー更新.
        getMouseInput();                    // マウス入力の受付.
        this.isGrounded = checkGrounded();	// 着地しているか判定.

        // 次の状態が決まっていなければ、プレイヤーの状態変化を調べて更新する.
        // 1.地面に着地していて、現在の状態が走行状態で、ジャンプボタン押下時 => ジャンプ状態へ遷移.
        // 2.地面に着地していて、現在の状態がジャンプ状態 => 走行状態へ遷移(ちょうど着地したところ).
        // 3.上記以外の状態の場合 => 次の状態は 何もなし のまま.
        if (this.next_step == STEP.NONE)
        {
            if (this.isGrounded)
            {
                switch (this.step)
                {
                    case STEP.RUN:
                        // ジャンプ入力時に空中に居たとしても.
                        // 入力が猶予時間内の場合、ジャンプ状態に遷移する.
                        // (入力がフライングしててもすぐジャンプする。これにより連続ジャンプが容易になる)
                        if (0.0f <= this.jump_timer && this.jump_timer <= JUMP_GRACE_TIME)
                        {
                            this.jump_timer = -1.0f;	// ボタンが押されていない ことを表す -1.0f に.
                            this.next_step = STEP.JUMP;	// 次の状態をジャンプ中 に.
                        }
                        break;
                    case STEP.JUMP:
                        this.next_step = STEP.RUN;      // 次の状態を走行中に変更.
                        break;
                }
            }
        }

        Vector2 new_velocity = this.rigidbody2D.velocity;	// 更新後の速度に、現在速度を設定.

        // 次の状態が決まっている場合(状態が変わった場合)、現在の状態を切り替える. 
        if(this.next_step != STEP.NONE)
        {
            this.step = this.next_step;	// 現在の状態 <= 次の状態.
            this.next_step = STEP.NONE;	// 次の状態   <= 状態なし.
            this.step_timer = 0.0f;	    // 状態が変化したので、ステップタイマーをゼロにリセット.

            // ジャンプ状態に切り替わった場合、ジャンプ開始.
            if(this.step == STEP.JUMP)
            {
                // ジャンプ音を再生.
                this.audio.PlayOneShot(jumpSound);	
                // ジャンプの高さからジャンプの初速を計算し、更新後の速度に反映.
                new_velocity.y = Mathf.Sqrt(2.0f * 9.8f * JUMP_HEIGHT_MAX);
                // 減速済みフラグ をクリアする.
                this.is_velocity_reduced = false;
            }
        }

        // もしジャンプ中なら、ジャンプの減速を行う.
        // ボタンを押してる間だけ初速を維持し(ただし重力は反映).
        // ボタンを離した時に減速させることで、ジャンプの強弱を制御する.
        if (this.step == STEP.JUMP)
        {
            // ボタンが離されていて、まだ減速していなくて、上昇中なら、減速する.
            if (!this.is_button_pressing && !this.is_velocity_reduced && new_velocity.y > 0.0f)
            {
                // 減速率を掛ける.
                new_velocity.y *= JUMP_KEY_RELEASE_REDUCE;
                // 減速済みフラグをONにする(二重に減速させてしまうことを防ぐため).
                this.is_velocity_reduced = true;
            }
        }

        // 向きを反映させた横方向の移動距離を計算.
        new_velocity.x = CalcHorizontalVelocity(this.move_speed);

        // Rigidbodyの速度を、上記で求めた速度で更新.
        // この行は、状態にかかわらず毎回実行される.
        this.rigidbody2D.velocity = new_velocity;

        // Animatorにフラグをセット.
        animator.SetBool("isGrounded", this.isGrounded);
        animator.SetFloat("vertical", this.GetComponent<Rigidbody2D>().velocity.y);

    }

    /// <summary>
    /// マウス入力を取得し、メンバ変数に値をセット.
    /// </summary>
    private void getMouseInput()
    {
        // マウスボタンが押されたらフラグをtrueにし、ジャンプタイマーを起動.
        if (Input.GetMouseButtonDown(0))
        {
            this.jump_timer = 0.0f;		// タイマーリセット.
            this.is_button_pressing = true;	// ボタンを押しているフラグをON.
        }
        // マウスを離した時、フラグをfalseにする.
        if (Input.GetMouseButtonUp(0))
        {
            this.is_button_pressing = false;	// ボタン押下フラグをOFF.
        }
    }

    /// <summary>
    /// タイマーの更新.
    /// </summary>
    private void updateTimer()
    {
        this.step_timer += Time.deltaTime;  // ステップタイマーに経過時間を加算.

        if (this.jump_timer >= 0.0f)        // ジャンプタイマーが起動している場合.
        {
            this.jump_timer += Time.deltaTime;	// ジャンプタイマーに経過時間を加算.
        }
    }

    /// <summary>
    /// 向いている方向に合わせて横方向の移動速度を計算する.
    /// </summary>
    /// <param name="speed">現在の移動速度</param>
    /// <returns>更新後の移動速度</returns>
    float CalcHorizontalVelocity(float speed)
    {
        if (!this.facingRight)
            speed = Mathf.Abs(speed) * -1;
        else
            speed = Mathf.Abs(speed);
        return speed;
    }

    /// <summary>
    /// 接地判定.
    /// </summary>
    /// <returns>接地しているかどうか.</returns>
    bool checkGrounded()
    {
        bool grounded = true;

        // 下方向にBoxCast発射.
        if (shotBoxCast(this.collider, Vector2.down, 0.1f, this.groundLayer))
        {
            // 下にものがある場合でも.
            // 現在の状態がジャンプ中かつ状態遷移のタイマーが0.45f未満のときは接地判定をfalseにする.
            // (ジャンプ直後のため).
            if (this.step == STEP.JUMP && this.step_timer < 0.45f) 
            {
                grounded = false;
            }
        }
        else
        {
            grounded = false;
        }

        return grounded;
    }

    /// <summary>
    /// BoxCastの射出.
    /// </summary>
    /// <param name="collider">基にするBoxCollider2D.</param>
    /// <param name="direction">方向.</param>
    /// <param name="distance">距離.</param>
    /// <param name="layer">レイヤーマスク.</param>
    /// <returns>射出した結果のRaycastHit2D</returns>
    private RaycastHit2D shotBoxCast(BoxCollider2D collider, Vector2 direction, float distance, LayerMask layer)
    {
        // コライダーの中心位置は上に0.5fずれているため、BoxCastを射出する位置も同様にずらす.
        Vector2 origin = (Vector2)this.transform.position + collider.offset;

        return Physics2D.BoxCast(origin, collider.size, 0, direction, distance, layer);
    }

    /// <summary>
    /// 壁に衝突したか判定する.
    /// </summary>
    /// <returns>壁に衝突したかどうか.</returns>
    bool isHitSide()
    {
        bool retval = false;

        // 左側と右側を見て、オブジェクトがあればtrueを返す.
        retval = shotBoxCast(this.collider, Vector2.left, 0.1f, this.groundLayer);
        if (!retval)
        {
            retval = shotBoxCast(this.collider, Vector2.right, 0.1f, this.groundLayer);
        }

        return retval;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("collision");

        // 側面にぶつかった場合は反転.
        if (isHitSide())
        {
            Debug.Log("side");
            this.facingRight = !this.facingRight;

            Quaternion rot = this.transform.rotation;
            this.transform.rotation = Quaternion.Euler(rot.x, facingRight ? 0 : 180, rot.z);
            this.audio.PlayOneShot(hitWallSound);	// 壁の衝突音を再生.
        }
        // 側面に当たっておらず、着地もしていない時は天井にぶつかった.
        else if (!checkGrounded())
        {	
            this.audio.PlayOneShot(hitWallSound);	// 壁の衝突音を再生.
        }
        // 側面に当たっておらず、着地している場合はポイント加算.
        else
        {
            // 最高到達点を更新した場合のみ,ポイント加算.
            // (微妙な座標の誤差を吸収するため、前座標+1.0fより大きい場合とする).
            if (this.transform.position.y > this.highest_position_y + 1.0f)
            {
                root.GetComponent<ScoreCounter>().AddScore(this.climbPoint);	// ポイント加算.

                this.highest_position_y = this.transform.position.y;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        // 当たった対象がコインならコインゲット処理.
        if (other.gameObject.CompareTag("Coin"))
        {
            GameObject.Destroy(other.gameObject);
            this.audio.PlayOneShot(coinSound);	// コインゲット音を再生.
            root.GetComponent<ScoreCounter>().AddScore(this.coinPoint);
        }
    }

    // 画面からいなくなった場合.
    void OnBecameInvisible()
    {
        Debug.Log("Falled");
        // 落下フラグをONにする.
        this.isFalled = true;
    }

}
