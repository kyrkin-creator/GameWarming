using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Спавн и Затемнение")]
    public Transform bedSpawnPoint;
    public GameObject player;
    public CanvasGroup fadeScreen;
    public CinemachineCamera CinemachineCam;

    [Header("Скример")]
    public RawImage jumpscareImage;     
    public AudioSource jumpscareAudio; 
    public float jumpscareTime = 1.5f; 

    [Header("Настройки респавна (Регулируй тут)")]
    public float blackScreenTime = 1.5f; // Сколько секунд висит черный экран
    public float fadeOutDuration = 2f;   // За сколько секунд экран проявляется (0 = мгновенно)
    public float cooldownTime = 2f;      // КД, чтобы враг не убил сразу же

    private bool isDead = false; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayerDied()
    {
        if (isDead) return; 
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        isDead = true; 

        // 1. Отключаем управление
        if (player.GetComponent<CharacterController>() != null)
            player.GetComponent<CharacterController>().enabled = false;
        
        // 2. Включаем скример
        if (jumpscareImage != null) jumpscareImage.gameObject.SetActive(true);
        if (jumpscareAudio != null) jumpscareAudio.Play();

        // 3. Ждем, пока игрок смотрит на скример
        yield return new WaitForSeconds(jumpscareTime);

        // 4. МГНОВЕННО ВЫКЛЮЧАЕМ СКРИМЕР И ДЕЛАЕМ ЭКРАН ЧЕРНЫМ
        if (jumpscareImage != null) jumpscareImage.gameObject.SetActive(false);
        if (fadeScreen != null) fadeScreen.alpha = 1f; // Резкий обрыв в черноту

        // 5. Ждем в темноте (настраивается в инспекторе)
        yield return new WaitForSeconds(blackScreenTime);
        
        // 6. Телепорт на кровать (пока экран черный)
        player.transform.position = bedSpawnPoint.position;
        player.transform.rotation = bedSpawnPoint.rotation;
        
        // 7. Возвращаем управление
        if (player.GetComponent<CharacterController>() != null)
            player.GetComponent<CharacterController>().enabled = true;
            
        // 8. ПЛАВНОЕ ПРОЯВЛЕНИЕ (Fade Out)
        if (fadeScreen != null && fadeOutDuration > 0)
        {
            float timer = 0f;
            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                // Линейно уменьшаем прозрачность от 1 до 0
                fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
                yield return null;
            }
            fadeScreen.alpha = 0f; // На всякий случай фиксируем полную прозрачность
        }
        else if (fadeScreen != null)
        {
            fadeScreen.alpha = 0f; // Если fadeOutDuration = 0, проявляем мгновенно
        }

        // 9. Кулдаун перед тем, как игрок снова станет уязвим
        yield return new WaitForSeconds(cooldownTime);
        isDead = false; 
    }
    public void TransitionToRoom(Transform destination, Transform playerTransform)
    {
        StartCoroutine(TransitionRoutine(destination, playerTransform));
    }

    IEnumerator TransitionRoutine(Transform destination, Transform playerTransform)
    {
        // 1. Мгновенно затемняем экран (или плавно, как хочешь)
        if (fadeScreen != null) fadeScreen.alpha = 1f;

        // 2. Отключаем управление, чтобы игрок не улетел
        if (playerTransform.GetComponent<CharacterController>() != null)
            playerTransform.GetComponent<CharacterController>().enabled = false;

        RenderSettings.fog = false;
        GameObject.Find("SnowEffect").SetActive(false);
        CinemachineCam.Lens.FarClipPlane = 50f;

        // 3. Ждем полсекунды в темноте
        yield return new WaitForSeconds(0.5f);

        // 4. Телепортируем игрока
        playerTransform.position = destination.position;
        // Если нужно, чтобы он смотрел в определенную сторону:
        playerTransform.rotation = destination.rotation;

        // 5. Ждем еще чуть-чуть, чтобы камера успела обновиться
        yield return new WaitForSeconds(0.5f);

        // 6. Включаем управление обратно
        if (playerTransform.GetComponent<CharacterController>() != null)
            playerTransform.GetComponent<CharacterController>().enabled = true;

        // 7. Плавно проявляем экран
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            if (fadeScreen != null) fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / 1f);
            yield return null;
        }
        if (fadeScreen != null) fadeScreen.alpha = 0f;
    }
}