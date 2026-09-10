using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Sentis;
using UnityEngine.Internal; // Sentis 관련 API

public struct ScoreMotion
{
    public string motionKey; // 모션 키 (예: "HeadScratch", "Hello")
    public double score;      // 유사도 점수

    public ScoreMotion(string key, double score)
    {
        this.motionKey = key;
        this.score = score;
    }
}

public class SBERT : MonoBehaviour
{
    private Model runtimeModel;
    private Worker worker;
    private BertTokenizer tokenizer;
    private string EMOTION = "중립";

    [Header("Threshold")]
    [SerializeField]
    private float actionThreshold = 0.9f;
    [SerializeField]
    private float faceThreshold = 0.75f;
    [SerializeField]
    private float npcActionThreshold = 0.6f;
    [SerializeField]
    private float npcFaceThreshold = 0.5f;

    // 일반 텍스트에 대한 임베딩 캐시
    private Dictionary<string, float[]> embeddingCache = new Dictionary<string, float[]>();
    Dictionary<string, float[]> precomputedEmbeddings = new Dictionary<string, float[]>();
    // 미리 임베딩할 기존 모션 문장들 (문장 -> 모션 키)
    public Dictionary<string, string> actMotionList = new Dictionary<string, string>();
    public Dictionary<string, string> faceMotionList = new Dictionary<string, string>();

    // 미리 계산된 임베딩 벡터 캐시 (문장 -> 임베딩 벡터)
    public Dictionary<string, List<float[]>> actMotionEmbeddings = new Dictionary<string, List<float[]>>();
    public Dictionary<string, List<float[]>> faceMotionEmbeddings = new Dictionary<string, List<float[]>>();

    // JSON 파일 경로 (절대 경로 예: Application.dataPath 기준)
    private static string ACTMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/ActMotion.json";
    private static string ACTMOTIONINFO_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/ActMotionInfo.json";
    private static string FACEMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/FaceMotion.json";

    //모션 문장 -> 모션 정보
    public Dictionary<string, MotionInfo> actMotionInfoList;

    void Awake()
    {
        Initialize();
        PrecomputeMotionEmbeddings();
    }

    /// <summary>
    /// 모델을 로드 및 초기화
    /// </summary>
    public void Initialize()
    {
        // Resources 폴더에서 "model" 에셋을 로드 (확장자 없이)
        ModelAsset asset = Resources.Load<ModelAsset>("model");
        if (asset == null)
        {
            Debug.LogError("Resources 폴더에서 'model' 에셋을 찾을 수 없습니다.");
            return;
        }
        runtimeModel = ModelLoader.Load(asset);

        // Sentis Worker 생성 (예제에서는 CPU 백엔드를 사용)
        worker = new Worker(runtimeModel, BackendType.CPU);

        // Resources 폴더 내의 vocab 파일 경로 (확장자 없이)
        tokenizer = new BertTokenizer("KR_SBERT_vocab");
    }

    private void LeadJsons()
    {
        actMotionInfoList = JsonFileReader.ReadMotionInfos(ACTMOTIONINFO_FILE_PATH);
        actMotionList = JsonFileReader.Read(ACTMOTION_FILE_PATH);
        faceMotionList = JsonFileReader.Read(FACEMOTION_FILE_PATH);
    }

    /// <summary>
    /// 앱 시작 시 기존 모션 문장들을 JSON 파일에서 읽어와 미리 임베딩 벡터를 계산해 저장합니다.
    /// JsonFileReader 클래스는 파일을 읽어 Dictionary&lt;string, string&gt;를 반환합니다.
    /// </summary>
    public void PrecomputeMotionEmbeddings()
    {
        LeadJsons();

        foreach (var kvp in actMotionList)
        {
            string sentence = kvp.Key;   // 문장
            string motionKey = kvp.Value; // "HeadScratch", "Hello" 등

            // MotionInfo 확인
            if (!actMotionInfoList.TryGetValue(motionKey, out MotionInfo info))
            {
                Debug.LogWarning($"ActMotionInfo.json에 '{motionKey}' 키가 없습니다.");
                continue;
            }

            // ▶ 해당 motionKey로 리스트 생성/가져오기
            if (!actMotionEmbeddings.TryGetValue(motionKey, out var list))
            {
                list = new List<float[]>();
                actMotionEmbeddings[motionKey] = list;
            }

            // 임베딩 계산 후 추가
            float[] embedding = GetEmbedding(sentence);
            list.Add(embedding);
        }

        // faceMotion은 기존 그대로 유지
        foreach (var kvp in faceMotionList)
        {
            string sentence = kvp.Key;   // 문장
            string motionKey = kvp.Value; // 표정

            // ▶ 해당 motionKey로 리스트 생성/가져오기
            if (!faceMotionEmbeddings.TryGetValue(motionKey, out var list))
            {
                list = new List<float[]>();
                faceMotionEmbeddings[motionKey] = list;
            }

            // 임베딩 계산 후 추가
            float[] embedding = GetEmbedding(sentence);
            list.Add(embedding);
        }
    }

    /// <summary>
    /// 두 텍스트 간의 코사인 유사도를 계산합니다.
    /// (입력 텍스트는 실시간으로 임베딩 벡터로 변환되고, 기존 문장들과 비교할 수 있습니다.)
    /// </summary>
    public ScoreMotion CompareWordText(string inputText, bool isAct, bool isPlayer = true)
    {
        if (string.IsNullOrEmpty(inputText) || inputText == "none")
            return new ScoreMotion("No match", 0.0);

        var inputEmbedding = GetEmbedding(inputText);
        double bestScore = -1.0;
        string bestMatchKey = "No match";

        if (isAct)
        {
            foreach (var kvp in actMotionEmbeddings)
            {
                string motionKey = kvp.Key;
                var embeddings = kvp.Value;
                var info = actMotionInfoList[motionKey];

                if (isPlayer)
                {
                    // 감정 제외 체크
                    if (!string.IsNullOrEmpty(info.emotionalExept[0]))
                    {
                        //var excluded = info.emotionalExept.Split(',').Select(e => e.Trim());
                        var excluded = info.emotionalExept;
                        if (excluded.Contains(EMOTION))
                            continue;
                    }
                }
                

                // 여러 임베딩 중 최고 유사도 판별
                foreach (var emb in embeddings)
                {
                    double score = CosineSimilarity(inputEmbedding, emb);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatchKey = motionKey;
                    }
                }
            }
        }
        else
        {
            foreach (var kvp in faceMotionEmbeddings)
            {
                string motionKey = kvp.Key;
                var embeddings = kvp.Value;

                // 여러 임베딩 중 최고 유사도 판별
                foreach (var emb in embeddings)
                {
                    double score = CosineSimilarity(inputEmbedding, emb);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatchKey = motionKey;
                    }
                }
            }
        }

        if (isPlayer)
        {
            if (isAct)
            {
                Debug.Log($"EMOTION: {EMOTION} (except: {actMotionInfoList[bestMatchKey].emotionalExept})");

                if (bestScore < actionThreshold)
                {
                    return new ScoreMotion(null, bestScore);
                }
            }
            else
            {
                if (bestScore < faceThreshold)
                {
                    return new ScoreMotion(null, bestScore);
                }
            }
        }
        else
        {
            if (isAct)
            {

                if (bestScore < npcActionThreshold)
                {
                    return new ScoreMotion(null, bestScore);
                }
            }
            else
            {
                if (bestScore < npcFaceThreshold)
                {
                    return new ScoreMotion(null, bestScore);
                }
            }
        }
        

        return new ScoreMotion(bestMatchKey, bestScore);
    }

    /// <summary>
    /// inputText와 유사한 actMotion의 MotionInfo 구조체를 반환하는 함수. Matching이 안되면 emotion 멤버가 "No match"라는 MotionInfo를 반환함.
    /// </summary>
    /// <param name="inputText"></param>
    /// <returns></returns>
    public MotionInfo GetActMotionInfo(string inputText, string emotion, bool isPlayer = true)
    {
        EMOTION = emotion;

        ScoreMotion scoreMotion = CompareWordText(inputText, true, isPlayer);
        if (scoreMotion.motionKey == null || scoreMotion.motionKey == "No match" || !actMotionInfoList.ContainsKey(scoreMotion.motionKey))
        {
            return new MotionInfo(null, null, 0);
        }

        MotionInfo motionInfo = actMotionInfoList[scoreMotion.motionKey];
        motionInfo.bestScore = scoreMotion.score; // 유사도 점수 업데이트

        return motionInfo;
    }

    /// <summary>
    /// inputText와 유사한 faceMotion 문자열을 반환.
    /// </summary>
    /// <param name="inputText"></param>
    /// <returns></returns>
    // public string GetFaceMotion(string inputText)
    // {
    //     return CompareWordText(inputText, false);
    // }

    /// <summary>
    /// 텍스트를 모델을 통해 임베딩 벡터로 변환합니다.
    /// 동일한 텍스트에 대해서는 캐싱하여 중복 계산을 피합니다.
    /// </summary>
    /// <param name="text">임베딩할 텍스트</param>
    /// <returns>임베딩된 벡터</returns>
    public float[] GetEmbedding(string text)
    {
        // 캐시에 이미 임베딩 결과가 있다면 반환합니다.
        if (embeddingCache.ContainsKey(text))
        {
            return embeddingCache[text];
        }

        // 토크나이저를 통해 토큰 ID 및 attention mask 배열 생성
        int[] tokenIds = tokenizer.Tokenize(text);
        int[] attentionMask = tokenizer.GetAttentionMask(tokenIds);
        int length = tokenIds.Length;

        // // "input_ids" 텐서 생성 (배치 크기 1, 길이: tokenIds.Length)
        // Tensor<int> inputIdsTensor = new Tensor<int>(
        //     new TensorShape(1, length),
        //     tokenIds);

        // // "attention_mask" 텐서 생성 (배치 크기 1)
        // // "input_ids" 정수형 텐서 생성 (배치 크기 1, 길이: tokenIds.Length)
        // Tensor<int> attentionMaskTensor = new Tensor<int>(
        //     new TensorShape(1, length),
        //     attentionMask);

        // "input_ids" 텐서 생성 (배치 크기 1, 길이: tokenIds.Length)
        Tensor<float> inputIdsTensor = new Tensor<float>(
            new TensorShape(1, length),
            tokenIds.Select(id => (float)id).ToArray());

        // "attention_mask" 텐서 생성 (배치 크기 1)
        Tensor<float> attentionMaskTensor = new Tensor<float>(
            new TensorShape(1, length),
            attentionMask.Select(val => (float)val).ToArray());

        // 텐서를 입력으로 설정하고 모델 실행
        worker.SetInput("input_ids", inputIdsTensor);
        worker.SetInput("attention_mask", attentionMaskTensor);
        worker.Schedule();

        // 출력 텐서를 PeekOutput()으로 참조합니다. (worker가 소유하므로 Dispose 불필요)
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        // 출력 텐서의 데이터가 준비될 때까지, Readback 요청 후 클론 텐서를 blocking 방식으로 가져옵니다.
        outputTensor.ReadbackRequest();
        Tensor<float> clonedOutput = outputTensor.ReadbackAndClone();

        int hiddenSize = clonedOutput.shape[1];
        float[] embedding = new float[hiddenSize];
        for (int i = 0; i < hiddenSize; i++)
        {
            embedding[i] = clonedOutput[0, i];
        }

        inputIdsTensor.Dispose();
        attentionMaskTensor.Dispose();
        clonedOutput.Dispose();

        // 결과를 캐시에 저장
        embeddingCache[text] = embedding;

        return embedding;
    }
    protected double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        double dotProduct = 0, normA = 0, normB = 0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}