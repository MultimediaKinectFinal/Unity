using UnityEngine;
using System; // 必填：為了使用事件監聽

public class AudioManager : MonoBehaviour
{
    [Header("音效檔案 (請在 Inspector 拖曳放入 AudioClip)")]
    public AudioClip shootClip;
    public AudioClip bounceClip;
    public AudioClip blockClip;
    public AudioClip penetrateClip;
    public AudioClip explosionClip;

    // --- 1. 綁定(訂閱)廣播頻道 ---
    private void OnEnable()
    {
        GameEvent.OnPlayerFire += PlayShootSound;
        GameEvent.OnShellBounce += PlayBounceSound;
        GameEvent.OnShellBlock += PlayBlockSound;
        GameEvent.OnArmorPenetrated += PlayPenetrateSound;
        GameEvent.OnEnemyDestroyed += PlayExplosionSound;
    }

    // --- 2. 解除(取消訂閱)廣播頻道，避免 Memory Leak ---
    private void OnDisable()
    {
        GameEvent.OnPlayerFire -= PlayShootSound;
        GameEvent.OnShellBounce -= PlayBounceSound;
        GameEvent.OnShellBlock -= PlayBlockSound;
        GameEvent.OnArmorPenetrated -= PlayPenetrateSound;
        GameEvent.OnEnemyDestroyed -= PlayExplosionSound;
    }

    // --- 3. 具體執行的音效邏輯 ---
    private void PlayShootSound()
    {
        // 假設玩家開火音效直接在攝影機位置播放
        AudioSource.PlayClipAtPoint(shootClip, Camera.main.transform.position);
    }

    private void PlayBounceSound(Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(bounceClip, pos);
    }

    private void PlayBlockSound(Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(blockClip, pos);
    }

    private void PlayPenetrateSound(Vector3 pos, string part, int damage)
    {
        AudioSource.PlayClipAtPoint(penetrateClip, pos);
    }

    // 多加了 GameObject tank 來對齊廣播頻道的格式
    private void PlayExplosionSound(GameObject tank, Vector3 pos, int score)
    {
        AudioSource.PlayClipAtPoint(explosionClip, pos);
    }
}