using LLMUnity;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Unity.VisualScripting;
using UnityEngine;

public class LlmManager : MonobehaviourSingleton<LlmManager>
{
    //LLM llm;
    private LLMCharacter myllmCharacter;
    public LLMCharacter MyLlmCharacter {get => myllmCharacter;}
    private LLM llm;

    protected override void Awake()
    {
            base.Awake();
            llm = GetComponent<LLM>();
            myllmCharacter = GetComponent<LLMCharacter>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        
        llm.SetModel("llama-3.2-3b-instruct-q4_k_m.gguf");
            llm.numThreads = -1;
            llm.numGPULayers = 10;
            myllmCharacter.llm = llm;

            myllmCharacter.SetPrompt(
            "You are an AI assistant that converts a user's spoken input (provided as Korean text) into a single, concise English sentence describing both the emotional state and corresponding physical actions (facial expressions and body movements). The output should only include the English sentence that conveys the mood and motion.\n\n" +
            "For each Korean input, generate a clear and concise sentence that includes:\n" +
            "1. **Act**: A physical action or behavior (e.g., jumping, sitting, waving).\n" +
            "2. **Face**: A facial expression (e.g., smiling, frowning, surprised).\n" +
            "3. **Emotion**: The feeling behind the action (e.g., happiness, anger, surprise).\n\n" +
            "Do not include any introductory text. **Only the action, face, and emotion should be included in the response.**\n\n" +
            "### Few-shot Examples ###\n\n" +
            "Input (Korean): \"오늘 정말 기뻐\"\n" +
            "Output (English): \"The person smiles broadly and claps their hands joyfully.\"\n\n" +
            "Input (Korean): \"너무 놀라서 숨이 막혀\"\n" +
            "Output (English): \"The person gasps in surprise, eyes widening and shoulders tensing.\"\n\n" +
            "Input (Korean): \"화가 나서 소리쳤어\"\n" +
            "Output (English): \"The person frowns deeply and shouts with an aggressive gesture, fists clenched.\"\n\n" +
            "Input (Korean): \"나 배고파\"\n" +
            "Output (English): \"The person looks slightly irritated, with their eyebrows furrowed, rubbing their stomach in discomfort.\"\n\n" +
            "Now, process the input and generate a response based on the input."
            );
            gameObject.SetActive(true);


            // 카메라 및 플레이어 오브젝트 설정
            //CameraController.Instance.SetTargetPlayer(gameObject);
            //ClientManager.Instance.MyPlayerObject = gameObject;

    }


}
