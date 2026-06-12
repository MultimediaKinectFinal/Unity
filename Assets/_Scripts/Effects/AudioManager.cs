using UnityEngine;
using System; 

public class AudioManager : MonoBehaviour
{
    [Header("()")]
    public AudioClip shootClip;
    public AudioClip bounceClip;
    public AudioClip blockClip;
    public AudioClip penetrateClip;
    public AudioClip explosionClip;

    private void OnEnable()
    {
        GameEvent.OnPlayerFire += PlayShootSound;
        GameEvent.OnShellBounce += PlayBounceSound;
        GameEvent.OnShellBlock += PlayBlockSound;
        GameEvent.OnArmorPenetrated += PlayPenetrateSound;
        GameEvent.OnEnemyDestroyed += PlayExplosionSound;
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerFire -= PlayShootSound;
        GameEvent.OnShellBounce -= PlayBounceSound;
        GameEvent.OnShellBlock -= PlayBlockSound;
        GameEvent.OnArmorPenetrated -= PlayPenetrateSound;
        GameEvent.OnEnemyDestroyed -= PlayExplosionSound;
    }


    private void PlayShootSound()
    {
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

    private void PlayExplosionSound(GameObject tank, Vector3 pos, int score)
    {
        AudioSource.PlayClipAtPoint(explosionClip, pos);
    }
}