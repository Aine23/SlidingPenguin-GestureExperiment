using UnityEngine;
using System.Collections.Generic;

public class InputDataManager : MonoBehaviour
{
    public static InputDataManager Instance;

    [HideInInspector]
    public InputData inputData;

    // inputs ã®åˆæœŸåŒ–ã¯ Start() ã§è¡Œã†ã€‚
    // Awake() ã ã¨ ExperimentManager.Awake() ã‚ˆã‚Šå…ˆã«èµ°ã‚‹å¯èƒ½æ€§ãŒã‚ã‚Šã€
    // IsInitialized ãŒ false ã®ã¾ã¾ DefaultInput ã«ãƒ•ã‚©ãƒ¼ãƒ«ãƒãƒƒã‚¯ã—ã¦ã—ã¾ã†ãŸã‚ã€‚
    private List<IInput> inputs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // å®Ÿé¨“ãƒ¢ãƒ¼ãƒ‰ãªã‚‰æ¡ä»¶ã«å¿œã˜ãŸå…¥åŠ›ã‚¯ãƒ©ã‚¹ã‚’ä½¿ç”¨ã—ã€
        // é€šå¸¸ãƒ—ãƒ¬ã‚¤ï¼ˆExperimentManagerãªã—ï¼‰ã¯DefaultInputã«ãƒ•ã‚©ãƒ¼ãƒ«ãƒãƒƒã‚¯ã™ã‚‹ã€‚
        IInput input =
            (ExperimentManager.Instance != null && ExperimentManager.Instance.IsInitialized)
            ? ExperimentManager.Instance.CreateInput()
            : new DefaultInput();

        inputs = new List<IInput> { input };

        Debug.Log($"[InputDataManager] å…¥åŠ›ã‚¯ãƒ©ã‚¹: {input.GetType().Name}");
    }

    private void Update()
    {
        foreach (IInput input in inputs)
        {
            input.UpdateInput();

            // Še“ü—Íƒf[ƒ^‚ğ InputData ‚É—¬‚µ‚Şi‘¼‚Ì“ü—Í‹@\‚Æ‹£‡‚µ‚È‚¢‚æ‚¤‚ÉA’l‚ğ‰ÁZ‚µ‚Ä—¬‚µ‚Şj
            inputData.direction = (inputData.direction + input.Direction).normalized;
            inputData.submit |= input.Submit;
            Instance.inputData.pause |= input.Pause;
        }
    }

    private void LateUpdate()
    {
        // Ÿ‚ÌƒtƒŒ[ƒ€‚Ì‚½‚ß‚ÉƒŠƒZƒbƒg‚·‚é
        inputData.ResetInput();
    }

    public void TriggerPauseInput()
    {
        inputData.pause = true;
    }
}
