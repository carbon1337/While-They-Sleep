/*

Handles the player lose jumpscare sequence.

Freezes the player, spawns a jumpscare enemy in front of
the camera, plays a sound, shakes the camera, then transitions
to the next scene.

*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance { get; private set; }

    [Header("References")]
    public Transform jumpscareCamera;
    public Transform jumpscareAnchor;
    public GameObject jumpscarePrefab;
    public AudioSource jumpscareAudioSource;
    public AudioClip jumpscareSFX;
    public GameObject gameplayUI;

    [Header("Timing")]
    public float scareDuration = 1f;
    public float shakeDuration = 0.25f;
    public float shakeStrength = 0.15f;

    private bool isJumpscareActive = false;

    #region Initialization

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    #region Jumpscare

    public void TriggerJumpscare()
    {
        //Check to ensure you aren't interrupting a jumpscare
        if (isJumpscareActive) return;

        StartCoroutine(JumpscareRoutine());
    }

    IEnumerator JumpscareRoutine()
    {
        isJumpscareActive = true;

        //Spawn jumpscare enemy at anchor in front of camera
        GameObject scareInstance = Instantiate(
            jumpscarePrefab,
            jumpscareAnchor.position,
            jumpscareAnchor.rotation
        );

        //Parent to camera so it stays locked in front of view
        scareInstance.transform.SetParent(jumpscareAnchor);

        //Disable game UI
        gameplayUI.SetActive(false);

        //Play scare sound
        if (jumpscareAudioSource != null && jumpscareSFX != null)
        {
            jumpscareAudioSource.PlayOneShot(jumpscareSFX);
        }

        //Play scare animation if animator exists
        Animator animator = scareInstance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Jumpscare");
        }

        //Shake camera
        yield return StartCoroutine(ShakeCamera());

        //Hold scare on screen
        yield return new WaitForSeconds(scareDuration);

        //Fade to lose screen
        ScreenFader.Instance.FadeToScene(0);
    }

    IEnumerator ShakeCamera()
    {
        if (jumpscareCamera == null) yield break;

        //Store original camera position so it can be restored later
        Vector3 originalLocalPos = jumpscareCamera.localPosition;

        float timer = 0f;

        //Shake camera until duration is reached
        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            //Strength decays over time so shake starts strong and settles
            float falloff = 1f - (timer / shakeDuration);

            //Random offset inside sphere scaled by shake strength and falloff
            Vector3 offset = Random.insideUnitSphere * shakeStrength * falloff;

            jumpscareCamera.localPosition = originalLocalPos + offset;

            yield return null;
        }

        //Reset camera to original position
        jumpscareCamera.localPosition = originalLocalPos;
    }

    #endregion
}